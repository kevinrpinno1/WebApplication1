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

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Customer API Controller is identical really to the Product Controller, commenting is omitted for brevity here
    /// except for the Delete method which has some business logic to prevent deletion of customers with orders
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateCustomerDto> _createValidator;
        private readonly IValidator<UpdateCustomerDto> _updateValidator;
        private readonly IMapper _mapper;


        public CustomerController(IValidator<UpdateCustomerDto> updateValidator, IValidator<CreateCustomerDto> createValidator, ApplicationDbContext context, IMapper mapper)
        {
            _updateValidator = updateValidator;
            _createValidator = createValidator;
            _context = context;
            _mapper = mapper;
        }

        // GET: api/customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCustomerDto>>> GetCustomers(CancellationToken ct)
        {
            var query = _context.Customers
                .AsNoTracking()
                .ProjectTo<GetCustomerDto>(_mapper.ConfigurationProvider);

            var customers = await query.ToListAsync(ct);

            return Ok(customers);
        }

        // GET: api/customer/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GetCustomerDto>> GetCustomersById(Guid id, CancellationToken ct)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Where(c => c.CustomerId == id)
                .ProjectTo<GetCustomerDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(ct);

            if (customer == null)
            {
                throw new EntityNotFoundException(string.Format(LoggingMessages.ExCustomerNotFound, id));
            }

            return Ok(customer);
        }

        // GET api/customer/{name}
        [HttpGet("{name}")]
        public async Task<ActionResult<IEnumerable<GetCustomerDto>>> GetCustomerByName(string name, CancellationToken ct)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Where(c => c.Name.ToUpper() == name.ToUpper())
                .ProjectTo<GetCustomerDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(customer);
        }

        // POST: api/customer
        [HttpPost]
        public async Task<ActionResult<GetCustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto, CancellationToken ct)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            HandleValidationFailure(validationResult);

            var customer = _mapper.Map<Customer>(dto);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<GetCustomerDto>(customer);

            return CreatedAtAction(nameof(GetCustomersById), new { id = customer.CustomerId }, resultDto);
        }

        // PUT: api/customer/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            HandleValidationFailure(validationResult);

            var customer = await _context.Customers.FindAsync(new object[] { id }, ct);

            if(customer == null)
                throw new EntityNotFoundException(string.Format(LoggingMessages.ExCustomerNotFound, id));

            _mapper.Map(dto, customer);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

        // don't want to allow deletion of customers that have orders
        // an improvement here might be to delete customers whose orders have a completed status only
        // and maybe only after x amount of time has passed since the order was completed - business logic would dictate this
        // deleting would have to cascade to orders and order items as well
        // DELETE: api/customer/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken ct)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == id, ct);

            if (customer == null)
            {
                throw new EntityNotFoundException(string.Format(LoggingMessages.ExCustomerNotFound, id));
            }

            if (customer.Orders.Any())
            {
                throw new BusinessLogicException(LoggingMessages.ExCustomerHasOrders);
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
