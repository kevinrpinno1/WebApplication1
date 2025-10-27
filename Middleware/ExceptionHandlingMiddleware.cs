using System.Text.Json;
using WebApplication1.Exceptions;

namespace WebApplication1.Middleware
{
    /// <summary>
    /// Custom exception handling middleware to catch and process exceptions globally.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// forms what the response will be for different exception types
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);

            context.Response.ContentType = "application/json";
            object result;

            switch (exception)
            {
                case ValidationAppException validationEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    result = validationEx.Errors;
                    break;
                case BusinessLogicException businessEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    result = new { error = businessEx.Message };
                    break;
                case EntityNotFoundException notFoundEx:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    result = new { error = notFoundEx.Message };
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    result = new { error = "An unexpected internal server error has occurred." };
                    break;
            }

            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}