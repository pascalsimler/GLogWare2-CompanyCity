using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class LogErpConfiguration : IEntityTypeConfiguration<LogErp>
{
    public void Configure(EntityTypeBuilder<LogErp> entity)
    {
        entity.HasKey(e => e.Guid);

        entity.Property(e => e.Guid)
              .IsRequired()
              .HasComment("Unique record identifier");
    }
}
