namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for returning product data to the client.
    /// </summary>
    public class GetProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}