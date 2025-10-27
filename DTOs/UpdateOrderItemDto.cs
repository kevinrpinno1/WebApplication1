namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for updating the quantity of an item in an existing order.
    /// </summary>
    public class UpdateOrderItemDto
    {
        public required int Quantity { get; set; }
    }
}