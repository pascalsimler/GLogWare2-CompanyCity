using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
              .HasMaxLength(64)
              .IsRequired()
              .HasComment("Permission Name");

        entity.Property(e => e.Description)
              .HasMaxLength(256)
              .IsRequired()
              .HasComment("Permission Description");
    }
}
