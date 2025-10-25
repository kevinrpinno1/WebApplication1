namespace WebApplication1.DTOs
{
    /// <summary>
    /// Defines the common properties for product-related data transfer objects.
    /// </summary>
    public interface IProductDto
    {
        string Name { get; set; }
        decimal Price { get; set; }
    }
}