namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for an item within a new order.
    /// </summary>
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}