using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class JobStatusConfiguration : IEntityTypeConfiguration<JobStatus>
{
    public void Configure(EntityTypeBuilder<JobStatus> entity)
    {
        entity.HasKey(e => e.Name);

        entity.Property(e => e.Name)
              .HasMaxLength(16)
              .IsRequired()
              .HasComment("Unique identifier for the JobStatus");
    }
}