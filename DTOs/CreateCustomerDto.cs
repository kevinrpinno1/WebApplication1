namespace WebApplication1.DTOs
{
    // --------------------------------------------------------------------
    // In doing this I did read up on records and why you would use them instead of classes
    // since this I think was more to assess current knowledge of C# and .NET I went with classes here
    // I see the main benefits of records are their immutability and concise syntax however
    // --------------------------------------------------------------------
    public class CreateCustomerDto : ICustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
