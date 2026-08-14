using Gudel.GLogWare.EFCore;
using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class LogPlcConfiguration : IEntityTypeConfiguration<LogPlc>
{
    public void Configure(EntityTypeBuilder<LogPlc> entity)
    {
        // Column properties
        entity.Property(e => e.Id)
            .HasComment("Unique record identifier");

        entity.Property(e => e.Status)
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasComment("Status of the transmission");

        entity.Property(e => e.Direction)
            .HasMaxLength(32)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Foreign key referencing PlcDirection.Name");
        
        entity.Property(e => e.Category)
            .HasMaxLength(32)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("Foreign key referencing PlcCategory.Name");

        entity.Property(e => e.Sender)
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasComment("Sender of the telegram");

        entity.Property(e => e.Receiver)
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasComment("Receiver of the telegram");

        entity.Property(e => e.Identifier)
            .HasMaxLength(4)
            .IsUnicode(false)
            .HasComment("Telegram identifier");

        entity.Property(e => e.Ackflag)
            .HasMaxLength(1)
            .IsUnicode(false)
            .HasComment("Acknowledge flag [0-1]");

        entity.Property(e => e.Counter)
            .HasMaxLength(1)
            .IsUnicode(false)
            .HasComment("Telegram counter [0-9]");
        
        entity.Property(e => e.Process)
            .HasMaxLength(32)
            .IsUnicode(false)
            .HasComment("Process/Service managing the telegram");

        entity.Property(e => e.Information)
            .HasMaxLength(1024)
            .HasComment("Additional Information");

        entity.Property(e => e.Data)
            .HasColumnType(DatabaseProviderHelper.GetBlobType())
            .HasComment("Telegram data");


        // Table relations
        entity.HasOne(e => e.PlcDirectionRecord)
           .WithMany(a => a.LogPlcs)
           .HasForeignKey(e => e.Direction)
           .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.PlcCategoryRecord)
            .WithMany(a => a.LogPlcs)
            .HasForeignKey(e => e.Category)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
