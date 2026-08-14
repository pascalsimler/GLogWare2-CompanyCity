using Gudel.GLogWare.UI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.UI.Infrastructure;

public class DictionaryConfiguration : IEntityTypeConfiguration<Dictionary>
{
    public void Configure(EntityTypeBuilder<Dictionary> entity)
    {
        // Coulumns properties
        entity.HasKey(e => new { e.DicoRef, e.Language });

        entity.Property(e => e.Language)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Language 2-letters code");

        entity.Property(e => e.Translation)
            .HasMaxLength(1024)
            .HasComment("Translation text");


        // Table relations
        entity.HasOne(e => e.LanguageRecord)
            .WithMany(a => a.Dictionaries)
            .HasForeignKey(e => e.Language)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
