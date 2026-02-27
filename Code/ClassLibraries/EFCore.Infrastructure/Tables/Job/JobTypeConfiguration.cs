using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class JobTypeConfiguration : IEntityTypeConfiguration<JobType>
{
    public void Configure(EntityTypeBuilder<JobType> entity)
    {
        entity.HasKey(e => e.Identifier);

        entity.Property(e => e.Identifier)
            .HasMaxLength(32)
            .IsRequired()
            .IsUnicode(false)
            .HasComment("Unique identifier for the JobStatus");

        entity.Property(e => e.TranslationKey)
            .HasMaxLength(32)
            .IsUnicode(false)
            .HasComment("Key for language translation");

        entity.Property(e => e.Description)
            .HasMaxLength(1024)
            .HasComment("Description - Comments");
    }
}
