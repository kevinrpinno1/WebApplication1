namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for creating a new product.
    /// </summary>
    public class CreateProductDto : IProductDto
    {
        public required string Name { get; set; } = string.Empty;
        public required decimal Price { get; set; }
        public int StockQuantity { get; set; } = 0;
    }
}