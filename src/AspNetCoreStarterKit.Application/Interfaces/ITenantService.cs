namespace AspNetCoreStarterKit.Application.Interfaces;

public interface ITenantService
{
    string? TenantId { get; }
    string? TenantName { get; }
    bool IsMultiTenant { get; }
}