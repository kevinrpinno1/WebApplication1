namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for creating a new product.
    /// </summary>
    public class CreateProductDto : IProductDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}