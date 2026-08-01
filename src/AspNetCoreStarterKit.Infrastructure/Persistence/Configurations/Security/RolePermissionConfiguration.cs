// AspNetCoreStarterKit.Infrastructure/Persistence/Configurations/Security/RolePermissionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AspNetCoreStarterKit.Domain.Entities.Security;

namespace AspNetCoreStarterKit.Infrastructure.Persistence.Configurations.Security;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(rp => rp.Id);
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
        builder.Property(rp => rp.GrantedAt).HasDefaultValueSql("GETDATE()");
        builder.HasQueryFilter(rp => rp.IsActive);
    }
}