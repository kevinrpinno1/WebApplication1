using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // ProductId is an identity column as defined in the ApplicationDbContext
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }


    }
}
