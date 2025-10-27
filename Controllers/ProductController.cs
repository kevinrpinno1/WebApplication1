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
    // Typically I would separate business logic into services to keep controllers thin, and this is done with OrdersController and OrderService
    // However, the overall logic isn't too complex for Products and Customers so keeping it all in the controller for simplicity. 

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;
        private readonly IMapper _mapper;

        public ProductController(ApplicationDbContext context, IValidator<CreateProductDto> createValidator, IValidator<UpdateProductDto> updateValidator, IMapper mapper)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _mapper = mapper;
        }

        // GET: api/product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetProductDto>>> GetProducts(CancellationToken ct)
        {
            // Retrieve all products from the database and project them to GetProductDto
            // as no tracking used since only doing a read operation
            // put to list async to execute the query
            var products = await _context.Products
                .AsNoTracking()
                .ProjectTo<GetProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetProductDto>> GetProductById(int id, CancellationToken ct)
        {
            // Retrieve a single product by its ID and project it to GetProductDto
            // where is done before first or default async to filter by id
            // single or default async used to ensure a proper response if not found
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId == id)
                .ProjectTo<GetProductDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(ct);

            if (product == null)
            {
                throw new EntityNotFoundException($"Product with ID {id} not found.");
            }

            return Ok(product);
        }

        // GET api/product/{name}
        [HttpGet("{name}")]
        public async Task<ActionResult<IEnumerable<GetProductDto>>> GetProductByName(string name, CancellationToken ct)
        {
            // same as ID but searching by name instead
            // to upper used to make case insensitive - String.Equals returns a 500 error, cannot be translated to SQL and used with LINQ to Entities
            // multiple products can have the same name so return a list
            var products = await _context.Products
                .AsNoTracking()
                .Where(c => c.Name.ToUpper() == name.ToUpper())
                .ProjectTo<GetProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(products);
        }

        // POST: api/product
        [HttpPost]
        public async Task<ActionResult<GetProductDto>> CreateProduct([FromBody] CreateProductDto dto, CancellationToken ct)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            HandleValidationFailure(validationResult); // Validate the incoming DTO and handle any validation failures using the base controller method

            var product = _mapper.Map<Product>(dto); // Map the validated DTO to a Product entity

            _context.Products.Add(product); // Add and track the new product entity
            await _context.SaveChangesAsync(ct); // Save changes to the database

            var resultDto = _mapper.Map<GetProductDto>(product); // Map the saved product entity back to a GetProductDto for the response

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, resultDto);
        }
        // PUT: api/product/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            HandleValidationFailure(validationResult);

            var existingProduct = await _context.Products.FindAsync(new object[] { id }, ct);

            if (existingProduct == null)
            {
                throw new EntityNotFoundException($"Product with ID {id} not found.");
            }

            _mapper.Map(dto, existingProduct);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct)
        {
            var existingProduct = await _context.Products.FindAsync(new object[] { id }, ct);

            if (existingProduct == null)
            {
                throw new EntityNotFoundException($"Product with ID {id} not found.");
            }

            _context.Products.Remove(existingProduct);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
