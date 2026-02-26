using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class JobStatusConfiguration : IEntityTypeConfiguration<JobStatus>
{
    public void Configure(EntityTypeBuilder<JobStatus> entity)
    {
        entity.HasKey(e => e.Identifier);

        entity.Property(e => e.Identifier)
              .HasMaxLength(32)
              .IsRequired()
              .IsUnicode(false)
              .HasComment("Unique identifier for the JobStatus");
    }
}