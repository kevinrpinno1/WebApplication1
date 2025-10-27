using AutoMapper;
using Data;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Services
{
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

        public async Task<Order> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.CreateOrder, dto.CustomerId);
            var order = _mapper.Map<Order>(dto);

            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(new object[] { item.ProductId }, ct);
                if (product == null)
                {
                    throw new BusinessLogicException($"Product with ID {item.ProductId} not found.");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new BusinessLogicException($"Not enough stock for Product ID {item.ProductId}. Available: {product.StockQuantity}, Requested: {item.Quantity}.");
                }

                product.StockQuantity -= item.Quantity;
                item.UnitPrice = product.Price;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(LoggingMessages.OrderCreated, order.OrderId, order.CustomerId);
            return order;
        }

        public async Task<Order> AddItemToOrderAsync(Guid orderId, CreateOrderItemDto itemDto, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.AddItemToOrder, itemDto.ProductId, orderId);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);

            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, orderId);
                throw new EntityNotFoundException("Order not found.");
            }

            var product = await _context.Products.FindAsync(new object[] { itemDto.ProductId }, ct);
            if (product == null) throw new BusinessLogicException($"Product with ID {itemDto.ProductId} not found.");

            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new BusinessLogicException($"Not enough stock for Product ID {itemDto.ProductId}. Available: {product.StockQuantity}, Requested: {itemDto.Quantity}.");
            }

            var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
            }
            else
            {
                var newItem = _mapper.Map<OrderItem>(itemDto);
                newItem.UnitPrice = product.Price - (itemDto.DiscountAmount ?? 0);
                order.OrderItems.Add(newItem);
            }

            product.StockQuantity -= itemDto.Quantity;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(LoggingMessages.OrderItemAdded, itemDto.ProductId, orderId);
            return order;
        }

        public async Task<Order> UpdateOrderItemAsync(Guid orderId, Guid orderItemId, UpdateOrderItemDto itemDto, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.UpdateOrderItem, orderItemId, orderId);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);

            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, orderId);
                throw new EntityNotFoundException("Order not found.");
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.OrderItemId == orderItemId);
            if (orderItem == null)
            {
                _logger.LogWarning(LoggingMessages.OrderItemNotFound, orderItemId, orderId);
                throw new EntityNotFoundException("Order item not found.");
            }
            if (orderItem.Product == null) throw new BusinessLogicException("Product information for this item is missing.");

            int quantityChange = itemDto.Quantity - orderItem.Quantity;

            if (quantityChange > 0 && orderItem.Product.StockQuantity < quantityChange)
            {
                throw new BusinessLogicException($"Not enough stock for Product ID {orderItem.ProductId}. Available: {orderItem.Product.StockQuantity}, Additional Requested: {quantityChange}.");
            }

            orderItem.Product.StockQuantity -= quantityChange;
            _mapper.Map(itemDto, orderItem);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(LoggingMessages.OrderItemUpdated, orderItemId, orderId);
            return order;
        }

        public async Task RemoveOrderItemAsync(Guid orderId, Guid orderItemId, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.RemoveOrderItem, orderItemId, orderId);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);

            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, orderId);
                throw new EntityNotFoundException("Order not found.");
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.OrderItemId == orderItemId);
            if (orderItem == null)
            {
                _logger.LogWarning(LoggingMessages.OrderItemNotFound, orderItemId, orderId);
                throw new EntityNotFoundException("Order item not found.");
            }

            if (orderItem.Product != null)
            {
                orderItem.Product.StockQuantity += orderItem.Quantity;
            }

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation(LoggingMessages.OrderItemRemoved, orderItemId, orderId);
        }

        public async Task DeleteOrderAsync(Guid id, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.DeleteOrder, id);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id, ct);

            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, id);
                throw new EntityNotFoundException("Order not found.");
            }

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

        public async Task UpdateOrderStatusAsync(Guid id, OrderStatus newStatus, CancellationToken ct)
        {
            _logger.LogInformation(LoggingMessages.UpdateOrderStatus, id, newStatus);
            var order = await _context.Orders.FindAsync(new object[] { id }, ct);
            if (order == null)
            {
                _logger.LogWarning(LoggingMessages.OrderNotFound, id);
                throw new EntityNotFoundException("Order not found.");
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation(LoggingMessages.OrderStatusUpdated, id, newStatus);
        }
    }
}