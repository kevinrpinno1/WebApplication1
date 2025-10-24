namespace WebApplication1.Models
{
    // PK OrderID GUID set by Database
    // FK CustomerID GUID referencing Customer table 
    // OrderDate set to current UTC date/time by Database
    // OrderItems navigation property for one-to-many relationship with OrderItem entity
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
        public DateTime OrderDate { get; set; } 

        public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
