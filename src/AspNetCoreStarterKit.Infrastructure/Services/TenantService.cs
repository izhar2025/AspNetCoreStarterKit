// AspNetCoreStarterKit.Infrastructure/Services/TenantService.cs
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? TenantId => _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;

    public string? TenantName => _httpContextAccessor.HttpContext?.User?.FindFirst("TenantName")?.Value;

    public bool IsMultiTenant => !string.IsNullOrEmpty(TenantId);
}