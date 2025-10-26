using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // PK OrderItemID GUID set by Database
    // FK OrderID GUID referencing Order table
    // FK ProductID int referencing Product table
    // Data Annotations enforce validation rules on Quantity and UnitPrice properties
    // Virtual navigation properties for Order and Product establish relationships
    public class OrderItem
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Range(0.00, (double)decimal.MaxValue, ErrorMessage = "Unit Price cannot be negative.")]
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
