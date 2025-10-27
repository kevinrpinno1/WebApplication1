using AutoMapper;
using Data;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    /// <summary>
    /// Order Service class to handle business logic related to orders and order items, allows for separation of concerns keeping controllers thin
    /// which improves maintainability and testability of the codebase
    /// 
    /// Logging was also added here and not in the controller to keep all order related operations and their logging in one place
    /// Logging messages are stored in a separate static class for consistency and ease of management and so the service here doesn't get too 
    /// cluttered with string literals. 
    /// Logging wasn't added to product or customer controllers as services were not created for those. Also done more for demonstration purposes.
    ///  
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, IMapper mapper, ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // CreateOrderAsync creates a new order based on the provided DTO
        // multiple order items can be included in the request, see postman collection for example
        // ex included here:
        //  "customerId": "5c5b4fea-3955-4de7-be9c-17a19f3bb200",
        //  "discountAmount": 10.00,
        //  "orderItems": [
        //        {
        //            "productId": 1,
        //            "quantity": 2,
        //            "discountAmount": 1.50
        //        },
        //        {
        //            "productId": 2,
        //            "quantity": 1
        //        }
        //    ]
        public async Task<Order> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.CreateOrder, dto.CustomerId);
            var order = _mapper.Map<Order>(dto); // Map the DTO to an Order entity using AutoMapper

            // loop through each order item to check stock and set unit price
            // throw exceptions if issues found
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(new object[] { item.ProductId }, ct);
                if (product == null)
                {
                    throw new BusinessLogicException(string.Format(LoggingMessages.ExProductNotFound, item.ProductId));
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new BusinessLogicException(string.Format(LoggingMessages.ExInsufficientStock, item.ProductId, product.StockQuantity, item.Quantity));
                }

                product.StockQuantity -= item.Quantity;
                item.UnitPrice = product.Price;
            }

            // Add the new order to the context and save changes to the database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(LoggingMessages.OrderCreated, order.OrderId, order.CustomerId);
            return order;
        }

        // AddItemToOrderAsync allows an item to be added to an existing order
        // If that item already exists in the order, its quantity is updated instead
        // otherwise a new order item is created within the order
        // A discount is also able to be applied to the unit price of the item if specified
        // stock levels are checked and updated accordingly as well with appropriate exceptions thrown if issues arise
        public async Task<Order> AddItemToOrderAsync(Guid orderId, CreateOrderItemDto itemDto, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.AddItemToOrder, itemDto.ProductId, orderId);
            // eagerly load the order with its items
            // singleOrDefault used to throw if multiple found which should never happen due to unique constraint on OrderId, but good practice
            // this could be optimized further by only loading what is needed, but for simplicity we load all items here
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);
            // throw if order not found
            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, orderId);
                throw new EntityNotFoundException(LoggingMessages.ExOrderNotFound);
            }
            // retrieve the product from the db to check stock and get price
            var product = await _context.Products.FindAsync(new object[] { itemDto.ProductId }, ct);
            if (product == null) throw new BusinessLogicException(string.Format(LoggingMessages.ExProductNotFound, itemDto.ProductId));

            // if we don't have enough stock, throw exception
            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new BusinessLogicException(string.Format(LoggingMessages.ExInsufficientStock, itemDto.ProductId, product.StockQuantity, itemDto.Quantity));
            }

            var existingItem = order.OrderItems.SingleOrDefault(oi => oi.ProductId == itemDto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity; // update quantity if item already exists in order
            }
            else
            {
                var newItem = _mapper.Map<OrderItem>(itemDto); // map DTO to new OrderItem entity
                newItem.UnitPrice = product.Price - (itemDto.DiscountAmount ?? 0); // apply discount if provided
                order.OrderItems.Add(newItem);
            }

            product.StockQuantity -= itemDto.Quantity; // reduce stock quantity
            await _context.SaveChangesAsync(ct); // save changes to db

            _logger.LogInformation(LoggingMessages.OrderItemAdded, itemDto.ProductId, orderId);
            return order;
        }

        // UpdateOrderItemAsync is similar to AddItemToOrderAsync but is different in execution
        // It allows the order item to have a new quantity specified directly, instead of simply adding to the existing quantity
        // As is done in AddItemToOrderAsync 
        // This would be used when already knowing that an order item exists and knowing the desired final quantity, no pesky maths needed
        // Stock levels are also checked and updated accordingly. Appropriate exceptions are thrown if issues arise.
        // The itemDto here does not allow for discount updates, only quantity updates -
        // so an enancement would be to allow discount updates as well if needed  for already existing orders items. 
        public async Task<Order> UpdateOrderItemAsync(Guid orderId, Guid orderItemId, UpdateOrderItemDto itemDto, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.UpdateOrderItem, orderItemId, orderId);
            // eagerly load the order with its items and associated products
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);

            // throw if order not found
            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, orderId);
                throw new EntityNotFoundException(LoggingMessages.ExOrderNotFound);
            }

            // find the specific order item to update
            var orderItem = order.OrderItems.SingleOrDefault(oi => oi.OrderItemId == orderItemId);
            if (orderItem == null)
            {
                _logger.LogWarning(LoggingMessages.OrderItemNotFound, orderItemId, orderId);
                throw new EntityNotFoundException(LoggingMessages.ExOrderItemNotFound);
            }
            if (orderItem.Product == null) throw new BusinessLogicException(LoggingMessages.ExMissingProductInfo);

            int quantityChange = itemDto.Quantity - orderItem.Quantity; // calculate the change in quantity

            if (quantityChange > 0 && orderItem.Product.StockQuantity < quantityChange)
            {
                throw new BusinessLogicException(string.Format(LoggingMessages.ExInsufficientStockForUpdate, orderItem.ProductId, orderItem.Product.StockQuantity, quantityChange));
            }

            orderItem.Product.StockQuantity -= quantityChange;
            _mapper.Map(itemDto, orderItem);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(LoggingMessages.OrderItemUpdated, orderItemId, orderId);
            return order;
        }

        // RemoveOrderItemAsync allows for the removal of an item from an existing order
        // follows the same pattern of eager loading the order with its items and associated products
        // and then finding the specific item to remove, checks are in place and proper exceptions thrown if issues arise
        public async Task RemoveOrderItemAsync(Guid orderId, Guid orderItemId, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.RemoveOrderItem, orderItemId, orderId);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .SingleOrDefaultAsync(o => o.OrderId == orderId, ct);

            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, orderId);
                throw new EntityNotFoundException(LoggingMessages.ExOrderNotFound);
            }

            var orderItem = order.OrderItems.SingleOrDefault(oi => oi.OrderItemId == orderItemId);
            if (orderItem == null)
            {
                _logger.LogWarning(LoggingMessages.OrderItemNotFound, orderItemId, orderId);
                throw new EntityNotFoundException(LoggingMessages.ExOrderItemNotFound);
            }

            // restock the product before removing the item from the order
            // this assumes that removing an item from an order means the item is no longer needed and has not shipped
            // is more likely to be the case than the restock being done down in the order delete method but 
            // should still be tied to business logic around order statuses in a real world scenario
            if (orderItem.Product != null)
            {
                orderItem.Product.StockQuantity += orderItem.Quantity;
            }

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation(LoggingMessages.OrderItemRemoved, orderItemId, orderId);
        }

        // DeleteOrderAsync allows for the deletion of an entire order
        // cascades to delete all associated order items as well
        // this is done by EF Core due to the configured relationships and delete behaviors set in ApplicationDbContext
        public async Task DeleteOrderAsync(Guid id, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.DeleteOrder, id);

            // eagerly load the order with its items and associated products
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .SingleOrDefaultAsync(o => o.OrderId == id, ct);

            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, id);
                throw new EntityNotFoundException(LoggingMessages.ExOrderNotFound);
            }

            // restock products before deleting the order
            // this should ultimately be tied to business logic around order statuses, so if the order is completed maybe we don't restock
            // but if it is pending or cancelled we do restock
            // added here for simplicity and demonstration purposes
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity += item.Quantity;
                }
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation(LoggingMessages.OrderDeleted, id);
        }

        // UpdateOrderStatusAsync allows updating the status of an existing order
        // simple operation, checks the order status and updates it accordingly
        public async Task UpdateOrderStatusAsync(Guid id, OrderStatus newStatus, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.UpdateOrderStatus, id, newStatus);
            var order = await _context.Orders.FindAsync(new object[] { id }, ct);
            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, id);
                throw new EntityNotFoundException(LoggingMessages.ExOrderNotFound);
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation(LoggingMessages.OrderStatusUpdated, id, newStatus);
        }
    }
}