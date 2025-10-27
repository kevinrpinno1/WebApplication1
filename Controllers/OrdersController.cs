using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Configuration;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    // Orders Controller has much of the business logic and data manipulation delegated to the OrderService to keep controller thin
    // The controller handles the requests and deals with validation and mapping to/from DTOs
    // The Get endpoints here are the basic same operations performed in Products and Customers controllers
    // The create and order items endpoints all utilize the OrderService
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateOrderDto> _createValidator;
        private readonly IValidator<CreateOrderItemDto> _createItemValidator;
        private readonly IValidator<UpdateOrderItemDto> _updateItemValidator;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;

        public OrdersController(ApplicationDbContext context, IValidator<CreateOrderDto> createValidator, IMapper mapper, IValidator<CreateOrderItemDto> createItemValidator, IValidator<UpdateOrderItemDto> updateItemValidator, IOrderService orderService)
        {
            _context = context;
            _createValidator = createValidator;
            _mapper = mapper;
            _createItemValidator = createItemValidator;
            _updateItemValidator = updateItemValidator;
            _orderService = orderService;
        }

        // GET : api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetOrderDto>>> GetOrders(CancellationToken ct)
        {
            // automapper projection to optimize query and mapping, eager loads related data as needed
            var orders = await _context.Orders
                .AsNoTracking()
                .ProjectTo<GetOrderDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(orders);
        }

        // GET : api/orders/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GetOrderDto>> GetOrderById(Guid id, CancellationToken ct)
        {
            // automapper projection to optimize query and mapping, eager loads related data as needed
            // single or default to handle not found case
            // order items and customer info are included via automapper configuration
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == id)
                .ProjectTo<GetOrderDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(ct);

            if (order == null)
            {
                throw new EntityNotFoundException(string.Format(LoggingMessages.ExOrderWithIdNotFound, id));
            }

            return Ok(order);
        }

        // GET api/orders/{name}
        [HttpGet("{name}")]
        public async Task<ActionResult<IEnumerable<GetOrderDto>>> GetOrderByCustomerName(string name, CancellationToken ct)
        {
            // while String.Comparison is reccomended, it does not translate to SQL in EF Core queries
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(c => c.Customer != null && c.Customer.Name.ToUpper() == name.ToUpper())
                .ProjectTo<GetOrderDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(orders);
        }

        // POST : api/orders
        [HttpPost]
        public async Task<ActionResult<GetOrderDto>> CreateOrder([FromBody] CreateOrderDto dto, CancellationToken ct)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            HandleValidationFailure(validationResult);

            var order = await _orderService.CreateOrderAsync(dto, ct);
            var getOrderDto = await _context.Orders
                .Where(o => o.OrderId == order.OrderId)
                .ProjectTo<GetOrderDto>(_mapper.ConfigurationProvider)
                .SingleAsync(ct);

            return CreatedAtAction(nameof(GetOrderById), new { id = getOrderDto.OrderId }, getOrderDto);
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken ct)
        {
            await _orderService.DeleteOrderAsync(id, ct);
            return NoContent();
        }

        // --- Order Item Endpoints ---

        // POST: api/orders/{orderId}/items
        [HttpPost("{orderId:guid}/items")]
        public async Task<ActionResult<GetOrderDto>> AddItemToOrder(Guid orderId, [FromBody] CreateOrderItemDto itemDto, CancellationToken ct)
        {
            var validationResult = await _createItemValidator.ValidateAsync(itemDto, ct);
            HandleValidationFailure(validationResult);

            var order = await _orderService.AddItemToOrderAsync(orderId, itemDto, ct);
            var getOrderDto = await _context.Orders
                .Where(o => o.OrderId == order.OrderId)
                .ProjectTo<GetOrderDto>(_mapper.ConfigurationProvider)
                .SingleAsync(ct);

            return Ok(getOrderDto);
        }

        // PUT: api/orders/{orderId}/items/{orderItemId}
        [HttpPut("{orderId:guid}/items/{orderItemId:guid}")]
        public async Task<ActionResult> UpdateOrderItem(Guid orderId, Guid orderItemId, [FromBody] UpdateOrderItemDto itemDto, CancellationToken ct)
        {
            var validationResult = await _updateItemValidator.ValidateAsync(itemDto, ct);
            HandleValidationFailure(validationResult);

            var order = await _orderService.UpdateOrderItemAsync(orderId, orderItemId, itemDto, ct);

            var getOrderDto = _mapper.Map<GetOrderDto>(order);

            return Ok(getOrderDto);
        }

        // DELETE: api/orders/{orderId}/items/{orderItemId}
        [HttpDelete("{orderId:guid}/items/{orderItemId:guid}")]
        public async Task<IActionResult> RemoveOrderItem(Guid orderId, Guid orderItemId, CancellationToken ct)
        {
            await _orderService.RemoveOrderItemAsync(orderId, orderItemId, ct);
            return NoContent();
        }

        // POST: api/orders/{id}/status
        [HttpPost("{id:guid}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatus newStatus, CancellationToken ct)
        {
            await _orderService.UpdateOrderStatusAsync(id, newStatus, ct);
            return NoContent();
        }
    }
}
