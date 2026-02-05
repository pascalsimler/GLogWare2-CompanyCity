using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> entity)
    {
        entity.HasKey(e => e.Name);

        entity.Property(e => e.Name)
              .HasMaxLength(16)
              .IsRequired()
              .HasComment("Unique Language name");
    }
}
