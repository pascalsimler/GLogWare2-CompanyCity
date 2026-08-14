using Gudel.GLogWare.UI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.UI.Infrastructure;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.Property(e => e.Login)
              .HasMaxLength(64)
              .IsUnicode(false)
              .IsRequired()
              .HasComment("Unique user login");

        entity.Property(e => e.DisplayName)
              .HasMaxLength(64)
              .HasComment("Display name of the user");

        entity.Property(e => e.PasswordHash)
              .HasMaxLength(64)
              .HasComment("Users password hash (null for LDAP users)");

        entity.Property(e => e.IsLdap)
              .HasComment("Mapped user from Active Directory (or more generally any LDAP)");

        entity.Property(e => e.IsLocked)
              .HasComment("FLag for locked users");

        entity.Property(e => e.LastSuccessfulLoginAt)
              .HasComment("Last time user successfully logged in");
    }
}
