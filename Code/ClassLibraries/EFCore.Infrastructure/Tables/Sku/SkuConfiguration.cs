using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Güdel.GLogWare.EFCore.Infrastructure;

public class SkuConfiguration : IEntityTypeConfiguration<Sku>
{
    public void Configure(EntityTypeBuilder<Sku> entity)
    {
        entity.HasKey(e => e.SkuId);

        entity.Property(e => e.SkuId)
              .HasMaxLength(16)
              .IsRequired()
              .HasComment("Unique identifier of the Sku - It is its barcode");

        entity.Property(e => e.Article)
              .HasMaxLength(16)
              .HasComment("Foreign key referencing Article.ArticleNumber");

        entity.Property(e => e.Place)
              .HasMaxLength(16)
              .HasComment("Foreign key referencing Place.Name");

        entity.HasOne(e => e.ArticleRecord)
              .WithMany(a => a.Skus)
              .HasForeignKey(e => e.Article)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.PlaceRecord)
              .WithMany(a => a.Skus)
              .HasForeignKey(e => e.Place)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
