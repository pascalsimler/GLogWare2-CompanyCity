using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.Property(e => e.Login)
              .HasMaxLength(64)
              .IsUnicode(false)
              .IsRequired()
              .HasComment("Unique user login");

        entity.Property(e => e.Login)
              .HasMaxLength(64)
              .HasComment("Unique user login");

        entity.Property(e => e.IsLdap)
              .HasComment("Mapped user from Active Directory (or more generally any LDAP)");
    }
}
