using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> entity)
    {
        entity.HasKey(e => e.Code);

        entity.Property(e => e.Code)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Language 2-letters code");

        entity.Property(e => e.Description)
            .HasMaxLength(1024)
            .HasComment("Language description");
    }
}
