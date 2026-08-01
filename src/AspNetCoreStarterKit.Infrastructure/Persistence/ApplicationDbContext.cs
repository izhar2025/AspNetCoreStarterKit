using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Common;
using AspNetCoreStarterKit.Domain.Entities;
using AspNetCoreStarterKit.Domain.Entities.Security;

namespace AspNetCoreStarterKit.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService,
            ITenantService tenantService)
            : base(options)
    {
        _currentUserService = currentUserService;
        _tenantService = tenantService;
    }


    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply multi-tenant and soft delete filters
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var expressions = new List<Expression>();

                // Soft delete filter
                var isActiveProperty = Expression.Property(parameter, "IsActive");
                var isActiveCondition = Expression.Equal(isActiveProperty, Expression.Constant(true));
                expressions.Add(isActiveCondition);

                // Multi-tenant filter
                if (_tenantService.IsMultiTenant && typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var tenantProperty = Expression.Property(parameter, "TenantId");
                    var tenantCondition = Expression.Equal(tenantProperty, Expression.Constant(_tenantService.TenantId));
                    expressions.Add(tenantCondition);
                }

                var combined = expressions.Aggregate(Expression.AndAlso);
                var lambda = Expression.Lambda(combined, parameter);
                entityType.SetQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            var currentUser = _currentUserService.UserId?.ToString() ?? "system";
            var tenantId = _tenantService.TenantId ?? "default";

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = DateTime.Now;
                entry.Entity.CreatedBy = currentUser;
                entry.Entity.IsActive = true;
                entry.Entity.TenantId = tenantId;  // ← AUTO SET TENANT
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOn = DateTime.Now;
                entry.Entity.ModifiedBy = currentUser;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}