namespace WebApplication1.Models
{
    // PK OrderItemID GUID set by Database
    // FK OrderID GUID referencing Order table
    // FK ProductID int referencing Product table
    // Virtual navigation properties for Order and Product establish relationships
    public class OrderItem
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
