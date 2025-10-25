namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for updating an existing product.
    /// </summary>
    public class UpdateProductDto : IProductDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}