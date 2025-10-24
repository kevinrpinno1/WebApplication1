using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApplication1.Configuration;
using WebApplication1.DTOs;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtSettings jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new IdentityUser { UserName = userDto.Email, Email = userDto.Email };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded) return Ok(new { Message = "User registered successfully." });

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
            //return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        [HttpGet("anon-token")]
        [AllowAnonymous]
        public async Task<IActionResult> AnonLoginUser()
        { 
            try
            {
                var users = _userManager.Users.ToList();
                if (!users.Any()) return NotFound(new { Message = "No demo users found in the database." });

                var random = new Random();
                var randomUser = users[random.Next(users.Count)];

                var token = await GenerateJwtToken(randomUser);
                return Ok(new { Token = token, User = randomUser});
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = "Failed to get Token" });
            }
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(userDto.Email);
            if (user == null) return Unauthorized(new { Message = "Invalid credentials." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, userDto.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var token = await GenerateJwtToken(user);

                return Ok(new { Token = token });
            }

            if (result.IsLockedOut) return Unauthorized(new { Message = "User account locked out." });

            return Unauthorized(new { Message = "Invalid credentials" });

        }

        // could be moved to a service for better separation of concerns
        public async Task<string> GenerateJwtToken(IdentityUser user)
        {
            //var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)                
            };

           // if roles needed
           // claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
