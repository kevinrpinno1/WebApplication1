using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApplication1.Configuration;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITokenService _tokenService; 

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var user = new IdentityUser { UserName = userDto.Email, Email = userDto.Email };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleConstants.User); // assigning a default user role here
                return Ok(new { Message = "User registered successfully." });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpGet("token")]
        [AllowAnonymous]
        public async Task<IActionResult> AnonLoginUser()
        {
            var users = await _userManager.Users.ToListAsync();
            if (!users.Any()) 
                return NotFound(new { Message = "No demo users found in the database." });

            var randomUser = users[Random.Shared.Next(users.Count)];

            return await GenerateTokenResponse(randomUser);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(userDto.Email!);

            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, userDto.Password, lockoutOnFailure: true);

                if (result.Succeeded)
                    return await GenerateTokenResponse(user);

                if (result.IsLockedOut)
                    return Unauthorized(new { Message = "User account locked out." });
            }

            return Unauthorized(new { Message = "Invalid credentials." });

        }

        private async Task<IActionResult> GenerateTokenResponse(IdentityUser user)
        {
            var token = await _tokenService.GenerateJwtToken(user);
            if(string.IsNullOrEmpty(token)) 
                return Unauthorized(new { Message = "Failed to generate token." });
            return Ok(new { access_token = token, User = user.Email });
        }
    }
}
