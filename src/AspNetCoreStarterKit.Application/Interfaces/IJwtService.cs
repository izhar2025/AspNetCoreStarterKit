// AspNetCoreStarterKit.Application/Interfaces/IJwtService.cs
using System.Security.Claims;
using AspNetCoreStarterKit.Domain.Entities.Security;

namespace AspNetCoreStarterKit.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, string roleName, string? tenantId = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}