using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> entity)
    {
        entity.Property(e => e.DecisionPlace)
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Route decision place");

        entity.Property(e => e.NextPlace)
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Next place candidate");


        // Table relations
        entity.HasOne(e => e.DecisionPlaceRecord)
            .WithMany(a => a.RouteDecisionPlaces)
            .HasForeignKey(e => e.DecisionPlace)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.DestinationPlaceRecord)
            .WithMany(a => a.RouteDestinationPlaces)
            .HasForeignKey(e => e.DestinationPlace)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.NextPlaceRecord)
            .WithMany(a => a.RouteNextPlaces)
            .HasForeignKey(e => e.NextPlace)
            .OnDelete(DeleteBehavior.Restrict);
    }
}