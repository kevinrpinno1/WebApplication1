namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for an item within a new order.
    /// </summary>
    public class CreateOrderItemDto
    {
        // nulls for those we don't need to specify when creating an order item
        // required for those we must specify
        public required int ProductId { get; set; }
        public required int Quantity { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}