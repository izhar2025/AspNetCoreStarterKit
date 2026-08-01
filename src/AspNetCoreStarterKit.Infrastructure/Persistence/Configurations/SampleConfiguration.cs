using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AspNetCoreStarterKit.Domain.Entities;

namespace AspNetCoreStarterKit.Infrastructure.Persistence.Configurations;

public class SampleConfiguration : IEntityTypeConfiguration<SampleEntity>
{
    public void Configure(EntityTypeBuilder<SampleEntity> builder)
    {
        builder.ToTable("SampleEntities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasQueryFilter(e => e.IsActive);
    }
}