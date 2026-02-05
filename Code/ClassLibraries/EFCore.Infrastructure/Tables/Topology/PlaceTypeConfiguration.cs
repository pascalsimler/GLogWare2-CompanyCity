using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Güdel.GLogWare.EFCore.Infrastructure;

public class PlaceTypeConfiguration : IEntityTypeConfiguration<PlaceType>
{
    public void Configure(EntityTypeBuilder<PlaceType> entity)
    {
        entity.HasKey(e => e.Name);

        entity.Property(e => e.Name)
              .HasMaxLength(16)
              .IsRequired()
              .HasComment("Unique identifier for the PlaceType");
    }
}