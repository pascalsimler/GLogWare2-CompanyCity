using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class ProtocolConfiguration : IEntityTypeConfiguration<Protocol>
{
    public void Configure(EntityTypeBuilder<Protocol> entity)
    {
        entity.HasKey(e => e.Guid);

        entity.Property(e => e.Guid)
              .IsRequired()
              .HasComment("Unique record identifier");
    }
}
