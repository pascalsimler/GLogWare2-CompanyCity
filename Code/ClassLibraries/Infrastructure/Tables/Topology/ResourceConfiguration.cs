using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> entity)
    {
        // Column properties
        entity.HasKey(e => e.Name);

        entity.Property(e => e.Name)
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Unique identifier for the resource");

        entity.Property(e => e.Mode)
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Working Mode of the resource");

        entity.Property(e => e.Parked)
            .HasComment("Flag indicating if the resource is parked");

        entity.Property(e => e.InfeedEnabled)
            .HasComment("Flag indicating if the resource is allowed to perform infeed orders");

        entity.Property(e => e.InfeedEnabled)
             .HasComment("Flag indicating if the resource is allowed to perform outfeed orders");

        entity.Property(e => e.InfeedEnabled)
             .HasComment("Flag indicating if the resource is allowed to perform rellocation orders");

        // Table relations
        entity.HasOne(e => e.ResourceModeRecord)
            .WithMany(a => a.Resources)
            .HasForeignKey(e => e.Mode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
