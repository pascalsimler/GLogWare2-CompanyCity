using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class LogPlcDirectionConfiguration : IEntityTypeConfiguration<LogPlcDirection>
{
    public void Configure(EntityTypeBuilder<LogPlcDirection> entity)
    {
        entity.HasKey(e => e.Identifier);

        entity.Property(e => e.Identifier)
            .HasMaxLength(32)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Unique identifier for the PlcDirection");
    }
}
