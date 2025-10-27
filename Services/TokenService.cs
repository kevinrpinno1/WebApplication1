using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApplication1.Configuration;

namespace WebApplication1.Services
{
    /// <summary>
    /// Service class for generating JWT tokens for users 
    /// uses IdentityUser from ASP.NET Core Identity for user management 
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<IdentityUser> _userManager;

        public TokenService(JwtSettings jwtSettings, UserManager<IdentityUser> userManager)
        {
            _jwtSettings = jwtSettings;
            _userManager = userManager;
        }
        /// <summary>
        /// this is called from the Auth Controller to generate a JWT token for the user
        /// auth controller handles user login and registration, already checks for valid credentials
        /// unless the anon token endpoint is hit, however that still uses a random valid user from the Identity store
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user); // get user roles from Identity store

            var claims = new List<Claim> // standard JWT claims
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            if (!string.IsNullOrEmpty(user.Email)) // null check for email in case somehow it's not set
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email)); // add email claim if available

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role))); // then add the role claims, can be multiple roles 

            var signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // details of what goes into the token
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
