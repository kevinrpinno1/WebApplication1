using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    // --------------------------------------------------------------------
    // In doing this I did read up on records and why you would use them instead of classes
    // since this I think was more to assess current knowledge of C# and .NET I went with classes here
    // I see the main benefits of records are their immutability and concise syntax however
    // --------------------------------------------------------------------
    public class UserDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
