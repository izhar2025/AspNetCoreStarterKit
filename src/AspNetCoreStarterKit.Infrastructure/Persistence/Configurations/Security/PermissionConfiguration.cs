// AspNetCoreStarterKit.Infrastructure/Persistence/Configurations/Security/PermissionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AspNetCoreStarterKit.Domain.Entities.Security;

namespace AspNetCoreStarterKit.Infrastructure.Persistence.Configurations.Security;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PermissionName).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Category).HasMaxLength(100);
        builder.Property(p => p.Module).HasMaxLength(50);
        builder.HasIndex(p => p.PermissionName).IsUnique();
        builder.HasQueryFilter(p => p.IsActive);
    }
}