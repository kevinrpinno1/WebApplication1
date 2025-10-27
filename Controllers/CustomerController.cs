using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Customer API Controller is identical really to the Product Controller, commenting is omitted for brevity here
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
                throw new EntityNotFoundException($"Customer with ID {id} not found.");
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
                throw new EntityNotFoundException($"Customer with ID {id} not found.");

            _mapper.Map(dto, customer);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

        // DELETE: api/customer/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken ct)
        {
            var customer = await _context.Customers.FindAsync(new object[] { id }, ct);

            if (customer == null)
            {
                throw new EntityNotFoundException($"Customer with ID {id} not found.");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
