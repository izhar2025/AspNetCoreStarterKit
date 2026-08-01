// AspNetCoreStarterKit.Infrastructure/Persistence/Configurations/Security/RoleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AspNetCoreStarterKit.Domain.Entities.Security;

namespace AspNetCoreStarterKit.Infrastructure.Persistence.Configurations.Security;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RoleName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.HasIndex(r => r.RoleName).IsUnique();
        builder.HasQueryFilter(r => r.IsActive);
    }
}