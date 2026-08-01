namespace AspNetCoreStarterKit.Application.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}