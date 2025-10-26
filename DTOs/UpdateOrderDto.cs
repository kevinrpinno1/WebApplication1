namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO for updating the items within an existing order.
    /// </summary>
    public class UpdateOrderDto
    {
        public List<CreateOrderItemDto> OrderItems { get; set; } = new();
    }
}