namespace WebApplication1.Exceptions
{
    /// <summary>
    /// Custom exception for handling validation failures. It carries a dictionary of errors
    /// to be returned to the client, allowing for structured error messages.
    /// </summary>
    public class ValidationAppException : Exception
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationAppException(IReadOnlyDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred.")
        {
            Errors = errors;
        }
    }
}