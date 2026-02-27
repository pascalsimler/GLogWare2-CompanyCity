using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Güdel.GLogWare.EFCore.Infrastructure;

public class PlaceConfiguration : IEntityTypeConfiguration<Place>
{
    public void Configure(EntityTypeBuilder<Place> entity)
    {
        entity.HasKey(e => e.Name);

        entity.Property(e => e.Name)
              .HasMaxLength(16)
              .IsUnicode(false)
              .IsRequired()
              .HasComment("Unique identifier for the place");

        entity.Property(e => e.Area)
              .HasMaxLength(16)
              .IsUnicode(false)
              .HasComment("Foreign key referencing Area.Name");

        entity.Property(e => e.PlaceType)
              .HasMaxLength(16)
              .IsUnicode(false)
              .HasComment("Foreign key referencing PlaceType.Name");


        // Table relations
        entity.HasOne(e => e.AreaRecord)
              .WithMany(a => a.Places)
              .HasForeignKey(e => e.Area)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.PlaceTypeRecord)
              .WithMany(a => a.Places)
              .HasForeignKey(e => e.PlaceType)
              .OnDelete(DeleteBehavior.Restrict);
    }
}