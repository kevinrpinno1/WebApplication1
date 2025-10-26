using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // ProductId is an identity column as defined in the ApplicationDbContext

    // data annotations commented out - see comment in Customer.cs
    public class Product
    {
        public int ProductId { get; set; }

        //[MinLength(2, ErrorMessage ="Product Name must be at least 2 characters long.")]
        public string Name { get; set; } = string.Empty;

       // [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }


    }
}
