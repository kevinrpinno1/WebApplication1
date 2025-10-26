namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for an item within a new order.
    /// </summary>
    public class CreateOrderItemDto
    {
        public required int ProductId { get; set; }
        public required int Quantity { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}