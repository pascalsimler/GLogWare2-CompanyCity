using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> entity)
    {
        // Column properties
        entity.HasKey(e => e.Jobid);

        entity.Property(e => e.Jobid)
            .HasMaxLength(16)
            .IsRequired()
            .HasComment("Unique JobId");

        entity.Property(e => e.Type)
            .HasMaxLength(32)
            .IsUnicode(false)
            .HasComment("Job type defined by JobType.Identifier");

        entity.Property(e => e.Status)
            .HasMaxLength(32)
            .IsUnicode(false)
            .HasComment("Job status defined by JobStatus.Identifier");

        entity.Property(e => e.SourcePlace)
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasComment("Source place defined by Place.Name");

        entity.Property(e => e.DestinationPlace)
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasComment("Destination place defined by Place.Name");

        entity.Property(e => e.ActualPlace)
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasComment("Actual place defined by Place.Name");

        entity.Property(e => e.NextPlace)
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasComment("Next place defined by Place.Name");

        entity.Property(e => e.Information)
            .HasMaxLength(1024)
            .HasComment("Information - Comments");


        // Table relations
        entity.HasOne(e => e.JobTypeRecord)
            .WithMany(a => a.Jobs)
            .HasForeignKey(e => e.Type)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.JobStatusRecord)
            .WithMany(a => a.Jobs)
            .HasForeignKey(e => e.Status)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.SourcePlaceRecord)
            .WithMany(a => a.JobSourcePlaces)
            .HasForeignKey(e => e.SourcePlace)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.DestinationPlaceRecord)
            .WithMany(a => a.JobDestinationPlaces)
            .HasForeignKey(e => e.DestinationPlace)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.ActualPlaceRecord)
            .WithMany(a => a.JobActualPlaces)
            .HasForeignKey(e => e.ActualPlace)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.NextPlaceRecord)
            .WithMany(a => a.JobNextPlaces)
            .HasForeignKey(e => e.NextPlace)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
