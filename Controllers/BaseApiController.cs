using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Exceptions;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Handles the result of a FluentValidation validation. If the result is invalid,
        /// it groups the errors and throws a ValidationAppException.
        /// </summary>
        /// <param name="validationResult">The result from a FluentValidation validator.</param>
        protected void HandleValidationFailure(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray());
                throw new ValidationAppException(errors);
            }
        }
    }
}