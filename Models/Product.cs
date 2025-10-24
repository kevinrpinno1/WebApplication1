using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // ProductId is an identity column as defined in the AppDbContext
    // Data Annotations are used to enforce validation rules on Name and Price properties
    // Annotations provide error messages for invalid data input, reduces complexity in application layer validation
    public class Product
    {
        public int ProductId { get; set; }

        [MinLength(2, ErrorMessage ="Product Name must be at least 2 characters long.")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }
        
    }
}
