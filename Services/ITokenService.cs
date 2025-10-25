using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Services
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(IdentityUser user);
    }
}
