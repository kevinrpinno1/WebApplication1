namespace WebApplication1.DTOs
{
    public class UpdateCustomerDto : ICustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
