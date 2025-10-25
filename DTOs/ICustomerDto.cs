namespace WebApplication1.DTOs
{
    public interface ICustomerDto
    {
        string Name { get; set; }
        string? Address { get; set; }
        string? PhoneNumber { get; set; }
    }
}
