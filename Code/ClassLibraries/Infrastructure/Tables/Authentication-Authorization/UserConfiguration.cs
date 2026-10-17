using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.UserName)
              .HasMaxLength(128)
              .IsRequired()
              .HasComment("User Name");

        entity.Property(e => e.Email)
              .HasMaxLength(128)
              .IsRequired()
              .HasComment("User Email");

        entity.Property(e => e.DisplayName)
              .HasMaxLength(128)
              .IsRequired()
              .HasComment("User Display Name");
    }
}
