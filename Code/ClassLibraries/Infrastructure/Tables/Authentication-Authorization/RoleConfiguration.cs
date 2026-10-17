using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
              .HasMaxLength(64)
              .IsRequired()
              .HasComment("Role Name");

        entity.Property(e => e.Description)
              .HasMaxLength(256)
              .IsRequired()
              .HasComment("Role Description");
    }
}
