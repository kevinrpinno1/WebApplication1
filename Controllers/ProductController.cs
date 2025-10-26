using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
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
            var query = _context.Products
                .AsNoTracking()
                .ProjectTo<GetProductDto>(_mapper.ConfigurationProvider);

            var products = await query.ToListAsync(ct);

            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetProductDto>> GetProductById(int id, CancellationToken ct)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId == id)
                .ProjectTo<GetProductDto>(_mapper.ConfigurationProvider);

            var product = await query.FirstOrDefaultAsync(ct);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // GET api/product/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<GetProductDto>> GetProductByName(string name, CancellationToken ct)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(c => c.Name.ToLower() == name.ToLower())
                .ProjectTo<GetProductDto>(_mapper.ConfigurationProvider);

            var product = await query.FirstOrDefaultAsync(ct);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // POST: api/product
        [HttpPost]
        public async Task<ActionResult<GetProductDto>> CreateProduct([FromBody] CreateProductDto dto, CancellationToken ct)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var product = _mapper.Map<Product>(dto);

            _context.Products.Add(product);
            await _context.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<GetProductDto>(product);

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, resultDto);
        }
        // PUT: api/product/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var existingProduct = await _context.Products.FindAsync(new object[] { id }, ct);

            if (existingProduct == null)
            {
                return NotFound();
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
                return NotFound();
            }

            _context.Products.Remove(existingProduct);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
