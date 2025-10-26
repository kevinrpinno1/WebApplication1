namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for returning order data to the client.
    /// </summary>
    public class GetOrderDto
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<GetOrderItemDto> OrderItems { get; set; } = new();

        public decimal Subtotal { get; set; } 
        public decimal OrderDiscount { get; set; } 
        public decimal OrderTotal { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}