namespace WebApplication1.Exceptions
{
    /// <summary>
    /// Exception thrown for violations of business rules (e.g., insufficient stock).
    /// </summary>
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException(string message) : base(message) { }
    }
}