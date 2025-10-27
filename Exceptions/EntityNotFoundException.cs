namespace WebApplication1.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested entity is not found in the database.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message) { }
    }
}