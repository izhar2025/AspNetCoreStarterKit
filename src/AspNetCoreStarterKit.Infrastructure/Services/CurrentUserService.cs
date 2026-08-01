using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    public bool IsAuthenticated => UserId.HasValue;
}