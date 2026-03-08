using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> entity)
    {
        // Column properties
        entity.HasKey(e => e.Name);

        entity.Property(e => e.Name)
            .HasMaxLength(32)
            .IsRequired()
            .IsUnicode(false)
            .HasComment("Unique parameter name");

        entity.Property(e => e.Type)
            .HasMaxLength(16)
            .IsRequired()
            .IsUnicode(false)
            .HasComment("Type from ParameterType.Identifier");

        entity.Property(e => e.DicoRef)
            .HasMaxLength(32)
            .IsUnicode(false)
            .HasComment("Translation from Dictionary table");

        entity.Property(e => e.MinValue)
            .HasMaxLength(64)
            .IsUnicode(false)
            .HasComment("Minimal allowed value");

        entity.Property(e => e.MaxValue)
            .HasMaxLength(64)
            .IsUnicode(false)
            .HasComment("Maximum allowed value");

        entity.Property(e => e.Value)
            .HasMaxLength(64)
            .HasComment("Parameter value");


        // Table relations
        entity.HasOne(e => e.ParameterTypeRecord)
            .WithMany(a => a.Parameters)
            .HasForeignKey(e => e.Type)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
