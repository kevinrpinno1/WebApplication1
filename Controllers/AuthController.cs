using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Configuration;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IValidator<UserDto> _userValidator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ITokenService tokenService, IValidator<UserDto> userValidator, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _userValidator = userValidator;
            _logger = logger;
        }

        /// <summary>
        /// Provides user registration functionality.
        /// Email and Password are required, validation occurs with FluentValidation and the Identity framework settings
        /// that are defined in Program.cs
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            var validationResult = await _userValidator.ValidateAsync(userDto); // validate the input against FluentValidation rules
            HandleValidationFailure(validationResult);

            // Create a new IdentityUser and then attempt to create it in the Identity store
            // this is tested against the password policies defined in Program.cs during Identity setup
            var user = new IdentityUser { UserName = userDto.Email, Email = userDto.Email };
            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                HandleIdentityResultFailure(result, user.Email!);
            }

            _logger.LogInformation("User {Email} registered successfully.", user.Email);
            await _userManager.AddToRoleAsync(user, RoleConstants.User); // assigning a default role of User here
            return Ok(new { Message = "User registered successfully." });
        }

        /// <summary>
        /// Anon login endpoint for demo purposes only
        /// grabs a random user from the db and uses those credentials to generate a token
        /// </summary>
        /// <returns></returns>
        [HttpGet("token")]
        [AllowAnonymous]
        public async Task<IActionResult> AnonLoginUser()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users.Count == 0)
                throw new EntityNotFoundException("No demo users found in the database.");

            var randomUser = users[Random.Shared.Next(users.Count)];
            _logger.LogInformation("Generating anonymous token for user {Email}", randomUser.Email);
            return await GenerateTokenResponse(randomUser);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody] UserDto userDto)
        {
            var validationResult = await _userValidator.ValidateAsync(userDto); // validate the input first to make sure correct format
            HandleValidationFailure(validationResult);

            var user = await _userManager.FindByEmailAsync(userDto.Email!); // now find if the user exists

            if (user != null)
            {
                // Check the password using SignInManager to leverage lockout features
                var result = await _signInManager.CheckPasswordSignInAsync(user, userDto.Password, lockoutOnFailure: true);

                if (result.Succeeded) // send a token back
                {
                    _logger.LogInformation("User {Email} logged in successfully.", user.Email);
                    return await GenerateTokenResponse(user);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account {Email} is locked out.", user.Email);
                    throw new BusinessLogicException("User account locked out.");
                }
            }
            
            _logger.LogWarning("Invalid login attempt for {Email}.", userDto.Email);
            throw new BusinessLogicException("Invalid credentials.");
        }

        /// <summary>
        /// helper method to handle token generation and response
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<IActionResult> GenerateTokenResponse(IdentityUser user)
        {
            var token = await _tokenService.GenerateJwtToken(user);
            if(string.IsNullOrEmpty(token)) 
                return Unauthorized(new { Message = "Failed to generate token." });
            return Ok(new { access_token = token, User = user.Email });
        }

        /// <summary>
        /// Handle IdentityResult failures by grouping errors and throwing a ValidationAppException
        /// </summary>
        /// <param name="identityResult"></param>
        /// <param name="email"></param>
        /// <exception cref="ValidationAppException"></exception>
        private void HandleIdentityResultFailure(IdentityResult identityResult, string email)
        {
            // Group Identity errors by code and collect descriptions into arrays and then into a dictionary
            var identityErrors = identityResult.Errors
                .GroupBy(e => e.Code, e => e.Description)
                .ToDictionary(g => g.Key, g => g.ToArray());

            foreach (var error in identityResult.Errors)
            {
                _logger.LogWarning("User registration failed for {Email}. Code: {Code}, Description: {Description}", email, error.Code, error.Description);
            }

            throw new ValidationAppException(identityErrors);
        }
    }
}
