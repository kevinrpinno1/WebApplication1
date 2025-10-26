namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for updating an existing product.
    /// </summary>
    public class UpdateProductDto : IProductDto
    {
        public required string Name { get; set; } = string.Empty;
        public required decimal Price { get; set; }
    }
}