using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace TransitHub.Services.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(IdentityUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        IEnumerable<string> GetRolesFromToken(string token);
    }
}