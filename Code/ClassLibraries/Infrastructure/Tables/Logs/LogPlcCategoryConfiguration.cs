using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class LogPlcCategoryConfiguration : IEntityTypeConfiguration<LogPlcCategory>
{
    public void Configure(EntityTypeBuilder<LogPlcCategory> entity)
    {
        entity.HasKey(e => e.Identifier);

        entity.Property(e => e.Identifier)
              .HasMaxLength(32)
              .IsUnicode(false)
              .IsRequired()
              .HasComment("Unique identifier for the PLC category");
    }
}