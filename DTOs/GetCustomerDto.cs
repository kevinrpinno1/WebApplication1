namespace WebApplication1.DTOs
{
    public class GetCustomerDto
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
