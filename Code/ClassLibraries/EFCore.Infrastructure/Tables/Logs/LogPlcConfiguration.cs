using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class LogPlcConfiguration : IEntityTypeConfiguration<LogPlc>
{
    public void Configure(EntityTypeBuilder<LogPlc> entity)
    {
        entity.Property(e => e.Id)
            .HasComment("Unique record identifier");

        entity.Property(e => e.Category)
            .HasMaxLength(32)
            .IsRequired()
            .IsUnicode(false)
            .HasComment("Foreign key referencing PlcCategory.Name");

        entity.HasOne(e => e.PlcCategoryRecord)
            .WithMany(a => a.LogPlcs)
            .HasForeignKey(e => e.Category)
            .OnDelete(DeleteBehavior.Restrict);

        entity.Property(e => e.Direction)
            .HasMaxLength(32)
            .IsRequired()
            .IsUnicode(false)
            .HasComment("Foreign key referencing PlcDirection.Name");

        entity.HasOne(e => e.PlcDirectionRecord)
            .WithMany(a => a.LogPlcs)
            .HasForeignKey(e => e.Direction)
            .OnDelete(DeleteBehavior.Restrict);

        entity.Property(e => e.Data)
            .HasColumnType(DatabaseProviderHelper.GetBlobType())
            .HasComment("Telegram data");
    }
}
