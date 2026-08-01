// AspNetCoreStarterKit.Infrastructure/Persistence/ApplicationDbContextSeed.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;

namespace AspNetCoreStarterKit.Infrastructure.Persistence;

public class ApplicationDbContextSeed
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextSeed> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationDbContextSeed(
        ApplicationDbContext context,
        ILogger<ApplicationDbContextSeed> logger,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            await SeedPermissionsAsync(cancellationToken);
            await SeedRolesAsync(cancellationToken);
            await SeedUsersAsync(cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Permissions
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Permissions.AnyAsync(cancellationToken))
            return;

        var permissions = new List<Permission>
        {
            // Dashboard
            new() { PermissionName = "ViewDashboard", Category = "Dashboard", Module = "Dashboard", Description = "View main dashboard" },
            new() { PermissionName = "ViewReports", Category = "Reports", Module = "Reports", Description = "View all reports" },
            
            // Users
            new() { PermissionName = "ManageUsers", Category = "User Management", Module = "Admin", Description = "Create, update, delete users" },
            new() { PermissionName = "ViewUsers", Category = "User Management", Module = "Admin", Description = "View user list" },
            new() { PermissionName = "ManageRoles", Category = "User Management", Module = "Admin", Description = "Manage roles and permissions" },
            new() { PermissionName = "ViewRoles", Category = "User Management", Module = "Admin", Description = "View roles" },
            new() { PermissionName = "ViewPermissions", Category = "User Management", Module = "Admin", Description = "View permissions" },
            
            // Core
            new() { PermissionName = "ManageCountries", Category = "Core", Module = "Admin", Description = "Manage countries" },
            new() { PermissionName = "ManageCities", Category = "Core", Module = "Admin", Description = "Manage cities" },
            new() { PermissionName = "ManageLocations", Category = "Core", Module = "Admin", Description = "Manage locations" },
            new() { PermissionName = "ManageCompanies", Category = "Core", Module = "Admin", Description = "Manage companies" },
            
            // Operations
            new() { PermissionName = "ManageParticipants", Category = "Participants", Module = "Operations", Description = "Manage participants" },
            new() { PermissionName = "ViewParticipants", Category = "Participants", Module = "Operations", Description = "View participants" },
            new() { PermissionName = "ManageVehicles", Category = "Vehicles", Module = "Operations", Description = "Manage vehicles" },
            new() { PermissionName = "ViewVehicles", Category = "Vehicles", Module = "Operations", Description = "View vehicles" },
            new() { PermissionName = "ManageShifts", Category = "Shifts", Module = "Operations", Description = "Manage shifts" },
            
            // Tracking
            new() { PermissionName = "ViewLiveTracking", Category = "Tracking", Module = "Monitoring", Description = "View live tracking" },
            new() { PermissionName = "ViewMovementHistory", Category = "Tracking", Module = "Monitoring", Description = "View movement history" },
            
            // Security
            new() { PermissionName = "BlacklistParticipants", Category = "Security", Module = "Security", Description = "Blacklist participants" },
            new() { PermissionName = "BlacklistVehicles", Category = "Security", Module = "Security", Description = "Blacklist vehicles" },
            new() { PermissionName = "ViewAuditLogs", Category = "Audit", Module = "Admin", Description = "View audit logs" },
        };

        await _context.Permissions.AddRangeAsync(permissions, cancellationToken);
        _logger.LogInformation("Seeded {Count} permissions", permissions.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Roles
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Roles.AnyAsync(cancellationToken))
            return;

        var allPermissions = await _context.Permissions.ToListAsync(cancellationToken);

        var roles = new List<Role>
        {
            new() { RoleName = "Admin", Description = "Full system access", IsSystemRole = true },
            new() { RoleName = "Operator", Description = "Daily operations access", IsSystemRole = true },
            new() { RoleName = "Viewer", Description = "Read-only access", IsSystemRole = true },
            new() { RoleName = "Security", Description = "Security monitoring", IsSystemRole = true },
        };

        await _context.Roles.AddRangeAsync(roles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Assign permissions
        var rolePermissions = new List<RolePermission>();

        var adminRole = roles.First(r => r.RoleName == "Admin");
        var operatorRole = roles.First(r => r.RoleName == "Operator");
        var viewerRole = roles.First(r => r.RoleName == "Viewer");
        var securityRole = roles.First(r => r.RoleName == "Security");

        // Admin gets all permissions
        rolePermissions.AddRange(allPermissions.Select(p => new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id }));

        // Operator gets operations permissions
        rolePermissions.AddRange(allPermissions
            .Where(p => p.Module == "Operations" || p.Module == "Monitoring")
            .Select(p => new RolePermission { RoleId = operatorRole.Id, PermissionId = p.Id }));

        // Viewer gets view-only permissions
        rolePermissions.AddRange(allPermissions
            .Where(p => p.PermissionName.StartsWith("View"))
            .Select(p => new RolePermission { RoleId = viewerRole.Id, PermissionId = p.Id }));

        // Security gets tracking and blacklist permissions
        rolePermissions.AddRange(allPermissions
            .Where(p => p.Category == "Tracking" || p.Category == "Security" || p.Module == "Monitoring")
            .Select(p => new RolePermission { RoleId = securityRole.Id, PermissionId = p.Id }));

        await _context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
        _logger.LogInformation("Seeded {Count} roles with permissions", roles.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Users
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
            return;

        var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == "Admin", cancellationToken);
        var operatorRole = await _context.Roles.FirstAsync(r => r.RoleName == "Operator", cancellationToken);
        var viewerRole = await _context.Roles.FirstAsync(r => r.RoleName == "Viewer", cancellationToken);
        var securityRole = await _context.Roles.FirstAsync(r => r.RoleName == "Security", cancellationToken);

        var users = new List<User>
        {
            new()
            {
                Username = "admin",
                Email = "admin@AspNetCoreStarterKit.com",
                FullName = "System Administrator",
                PasswordHash = _passwordHasher.HashPassword("Admin@123"),
                RoleId = adminRole.Id,
                MustChangePassword = false,
                IsActive = true
            },
            new()
            {
                Username = "operator",
                Email = "operator@AspNetCoreStarterKit.com",
                FullName = "John Operator",
                PasswordHash = _passwordHasher.HashPassword("Operator@123"),
                RoleId = operatorRole.Id,
                MustChangePassword = true,
                IsActive = true
            },
            new()
            {
                Username = "viewer",
                Email = "viewer@AspNetCoreStarterKit.com",
                FullName = "Sarah Viewer",
                PasswordHash = _passwordHasher.HashPassword("Viewer@123"),
                RoleId = viewerRole.Id,
                MustChangePassword = true,
                IsActive = true
            },
            new()
            {
                Username = "security",
                Email = "security@AspNetCoreStarterKit.com",
                FullName = "Ahmed Security",
                PasswordHash = _passwordHasher.HashPassword("Security@123"),
                RoleId = securityRole.Id,
                MustChangePassword = true,
                IsActive = true
            }
        };

        await _context.Users.AddRangeAsync(users, cancellationToken);
        _logger.LogInformation("Seeded {Count} users", users.Count);
    }
}