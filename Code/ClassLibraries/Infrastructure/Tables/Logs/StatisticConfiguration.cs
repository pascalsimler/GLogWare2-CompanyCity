using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class StatisticConfiguration : IEntityTypeConfiguration<Statistic>
{
    public void Configure(EntityTypeBuilder<Statistic> entity)
    {
        // Column properties
        entity.HasKey(e => new { e.Category, e.Element, e.Day, e.Hour });

        entity.Property(e => e.Category)
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Statistic category");

        entity.Property(e => e.Element)
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Statistic element");

        // Table relations
        entity.HasOne(e => e.StatisticCategoryRecord)
           .WithMany(a => a.Statistics)
           .HasForeignKey(e => e.Category)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
