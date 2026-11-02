using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
              .HasComment("The unique identifier for the user");

        entity.Property(e => e.UserName)
              .HasMaxLength(128)
              .IsRequired()
              .HasComment("The username of the user to be used for login and identification");

        entity.Property(e => e.Email)
              .HasMaxLength(128)
              .IsRequired()
              .HasComment("The email address of the user. Can be used for login and identification");

        entity.Property(e => e.DisplayName)
              .HasMaxLength(128)
              .IsRequired()
              .HasComment("The display name of the user");

        entity.Property(e => e.Phone)
              .HasMaxLength(32)
              .HasComment("User Phone");

        entity.Property(e => e.Info1)
              .HasMaxLength(256)
              .HasComment("Additional information 1");

        entity.Property(e => e.Info2)
              .HasMaxLength(256)
              .HasComment("Additional information 2");

        entity.Property(e => e.Info3)
              .HasMaxLength(256)
              .HasComment("Additional information 3");

        entity.Property(e => e.ThumbnailPhotoContentType)
              .HasMaxLength(128)
              .HasComment("Thumbnail photo content type");
    }
}
