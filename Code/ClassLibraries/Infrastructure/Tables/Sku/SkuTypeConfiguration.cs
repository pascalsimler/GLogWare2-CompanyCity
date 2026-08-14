using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class SkuTypeConfiguration : IEntityTypeConfiguration<SkuType>
{
    public void Configure(EntityTypeBuilder<SkuType> entity)
    {
        entity.HasKey(e => e.Identifier);

        entity.Property(e => e.Identifier)
            .HasMaxLength(32)
            .IsRequired()
            .IsUnicode(false)
            .HasComment("Unique identifier for the SkuType");

        entity.Property(e => e.TranslationKey)
            .HasMaxLength(32)
            .IsUnicode(false)
            .HasComment("Key for language translation");

        entity.Property(e => e.Description)
            .HasMaxLength(1024)
            .HasComment("Description - Comments");
    }
}
