using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using System.Xml;

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

        for (int i = 1; i <= 20; i++)
        {
            entity.Property<int>($"Counter{i:00}")
                .HasDefaultValue(0)
                .HasComment($"Statistic Counter{i:00}");
        }

        // Table relations
        entity.HasOne(e => e.StatisticCategoryRecord)
           .WithMany(a => a.Statistics)
           .HasForeignKey(e => e.Category)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
