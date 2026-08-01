// AspNetCoreStarterKit.API/Middleware/PermissionMiddleware.cs
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AspNetCoreStarterKit.API.Attributes;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.API.Middleware;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        var endpoint = context.GetEndpoint();
        var permissionAttribute = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();

        if (permissionAttribute != null)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var userId = int.Parse(userIdClaim);
            var hasPermission = await HasPermission(userId, permissionAttribute.Permission, unitOfWork);

            if (!hasPermission)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync($"Forbidden: Missing permission '{permissionAttribute.Permission}'");
                return;
            }
        }

        await _next(context);
    }

    private static async Task<bool> HasPermission(int userId, string permission, IUnitOfWork unitOfWork)
    {
        var user = await unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.Role)
            .ThenInclude(r => r!.RolePermissions)
            .ThenInclude(rp => rp!.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Role?.RolePermissions == null)
            return false;

        return user.Role.RolePermissions
            .Any(rp => rp.Permission != null && rp.Permission.PermissionName == permission);
    }
}