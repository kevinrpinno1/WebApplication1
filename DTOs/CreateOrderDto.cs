namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for creating a new order.
    /// </summary>
    public class CreateOrderDto
    {
        // nulls for those we don't need to specify when creating an order item
        // required for those we must specify
        public required Guid CustomerId { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; } = new();
        public decimal? DiscountAmount { get; set; }
    }
}