using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class ParameterTypeConfiguration : IEntityTypeConfiguration<ParameterType>
{
    public void Configure(EntityTypeBuilder<ParameterType> entity)
    {
        entity.HasKey(e => e.Identifier);

        entity.Property(e => e.Identifier)
            .HasMaxLength(16)
            .IsRequired()
            .IsUnicode(false)
            .HasComment("Unique type indentifier");

        entity.Property(e => e.Description)
            .HasMaxLength(1024)
            .HasComment("Description - Comments");
    }
}
