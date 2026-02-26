using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class LogPlcDirectionConfiguration : IEntityTypeConfiguration<LogPlcDirection>
{
    public void Configure(EntityTypeBuilder<LogPlcDirection> entity)
    {
        entity.HasKey(e => e.Name);

        entity.Property(e => e.Name)
            .HasMaxLength(32)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Unique identifier for the PlcDirection");
    }
}
