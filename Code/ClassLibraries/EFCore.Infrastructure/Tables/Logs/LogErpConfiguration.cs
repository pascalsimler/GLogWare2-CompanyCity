using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

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
