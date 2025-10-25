namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for returning order data to the client.
    /// </summary>
    public class GetOrderDto
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<GetOrderItemDto> OrderItems { get; set; } = new();
    }
}