using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Configuration;
using WebApplication1.DTOs;
using WebApplication1.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateOrderDto> _createValidator;
        private readonly IValidator<CreateOrderItemDto> _createItemValidator;
        private readonly IValidator<UpdateOrderItemDto> _updateItemValidator;
        private readonly IMapper _mapper;
        public OrdersController(ApplicationDbContext context, IValidator<CreateOrderDto> createValidator, IMapper mapper, IValidator<CreateOrderItemDto> createItemValidator, IValidator<UpdateOrderItemDto> updateItemValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _mapper = mapper;
            _createItemValidator = createItemValidator;
            _updateItemValidator = updateItemValidator;
        }

        // GET : api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetOrderDto>>> GetOrders(CancellationToken ct)
        {
            var query = _context.Orders
                .AsNoTracking()
                .ProjectTo<GetOrderDto>(_mapper.ConfigurationProvider);

            var orders = await query.ToListAsync(ct);
            return Ok(orders);
        }

        // GET : api/orders/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GetOrderDto>> GetOrderById(Guid id, CancellationToken ct)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == id)
                .ProjectTo<GetOrderDto>(_mapper.ConfigurationProvider);

            var order = await query.FirstOrDefaultAsync(ct);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // POST : api/orders
        [HttpPost]
        public async Task<ActionResult<GetOrderDto>> CreateOrder([FromBody] CreateOrderDto dto, CancellationToken ct)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var order = _mapper.Map<Order>(dto);

            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(new object[] { item.ProductId }, ct);
                
                if (product == null)
                {
                    return BadRequest($"Product with ID {item.ProductId} not found.");
                }
                if (product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Not enough stock for Product ID {item.ProductId}. " +
                                      $"Available: {product.StockQuantity}, Requested: {item.Quantity}.");
                }

                product.StockQuantity -= item.Quantity;

                item.UnitPrice = product.Price;

            }

            _context.Orders.Add(order);
            await  _context.SaveChangesAsync(ct);

            var getOrderDto = await _context.Orders
                .Where(o => o.OrderId == order.OrderId)
                .ProjectTo<GetOrderDto>(_mapper.ConfigurationProvider)
                .FirstAsync(ct);

            return CreatedAtAction(nameof(GetOrderById), new { id = getOrderDto.OrderId }, getOrderDto);
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken ct)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

        // --- Order Item Endpoints ---

        // POST: api/orders/{orderId}/items
        [HttpPost("{orderId:guid}/items")]
        public async Task<ActionResult<GetOrderDto>> AddItemToOrder(Guid orderId, [FromBody] CreateOrderItemDto itemDto, CancellationToken ct)
        {
            var validationResult = await _createItemValidator.ValidateAsync(itemDto, ct);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
            if (order == null) return NotFound("Order not found.");

            var product = await _context.Products.FindAsync(new object[] { itemDto.ProductId }, ct);
            if (product == null) return BadRequest($"Product with ID {itemDto.ProductId} not found.");

            var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
            }
            else
            {
                var newItem = _mapper.Map<OrderItem>(itemDto);
                newItem.UnitPrice = product.Price;
                order.OrderItems.Add(newItem);
            }

            await _context.SaveChangesAsync(ct);
            var orderDto = _mapper.Map<GetOrderDto>(order);
            return Ok(orderDto);
        }

        // PUT: api/orders/{orderId}/items/{orderItemId}
        [HttpPut("{orderId:guid}/items/{orderItemId:guid}")]
        public async Task<IActionResult> UpdateOrderItem(Guid orderId, Guid orderItemId, [FromBody] UpdateOrderItemDto itemDto, CancellationToken ct)
        {
            var validationResult = await _updateItemValidator.ValidateAsync(itemDto, ct);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
            if (order == null) return NotFound("Order not found.");

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.OrderItemId == orderItemId);
            if (orderItem == null) return NotFound("Order item not found.");

            _mapper.Map(itemDto, orderItem);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

        // DELETE: api/orders/{orderId}/items/{orderItemId}
        [HttpDelete("{orderId:guid}/items/{orderItemId:guid}")]
        public async Task<IActionResult> RemoveOrderItem(Guid orderId, Guid orderItemId, CancellationToken ct)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
            if (order == null) return NotFound("Order not found.");

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.OrderItemId == orderItemId);
            if (orderItem == null) return NotFound("Order item not found.");

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

        // POST: api/orders/{id}/status
        [HttpPost("{id:guid}/status")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatus newStatus, CancellationToken ct)
        {
            var order = await _context.Orders.FindAsync(new object[] { id }, ct);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

    }
}
