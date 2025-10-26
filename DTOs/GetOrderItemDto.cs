namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for returning order item data to the client.
    /// </summary>
    public class GetOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; } 
        public decimal LineTotal { get; set; }
    }
}