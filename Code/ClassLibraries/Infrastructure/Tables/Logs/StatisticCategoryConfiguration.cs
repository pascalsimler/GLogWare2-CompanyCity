using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class StatisticCategoryConfiguration : IEntityTypeConfiguration<StatisticCategory>
{
    public void Configure(EntityTypeBuilder<StatisticCategory> entity)
    {
        entity.HasKey(e => e.Identifier);

        entity.Property(e => e.Identifier)
              .HasMaxLength(16)
              .IsUnicode(false)
              .IsRequired()
              .HasComment("Unique identifier for the statistic category");
    }
}