using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> entity)
    {
        entity.HasKey(e => e.JobId);

        entity.Property(e => e.JobId)
              .HasMaxLength(16)
              .IsRequired()
              .HasComment("Unique JobId");

        entity.Property(e => e.JobStatus)
              .HasMaxLength(16)
              .IsRequired()
              .HasComment("Foreign key referencing JobStatus.Name");

        entity.HasOne(e => e.JobStatusRecord)
              .WithMany(a => a.Jobs)
              .HasForeignKey(e => e.JobStatus)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
