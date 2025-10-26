namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for creating a new order.
    /// </summary>
    public class CreateOrderDto
    {
        public Guid CustomerId { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; } = new();
        public decimal? DiscountAmount { get; set; }
    }
}