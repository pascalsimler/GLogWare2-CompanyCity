using System;
using System.Collections.Generic;
using Gudel.GLogWare.Infrastructure.Views;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.Infrastructure;

public partial class GLogWareDbContext 
{
    public virtual DbSet<ViewInventory> ViewInventories { get; set; }

    public virtual DbSet<ViewStatisticBridgePerDay> ViewStatisticBridgePerDays { get; set; }

    public virtual DbSet<ViewStatisticBridgePerDayConsolidated> ViewStatisticBridgePerDayConsolidateds { get; set; }

    public virtual DbSet<ViewStatisticBridgePerHour> ViewStatisticBridgePerHours { get; set; }

    public virtual DbSet<ViewStatisticBridgePerHourConsolidated> ViewStatisticBridgePerHourConsolidateds { get; set; }

    public virtual DbSet<ViewStatisticFillLevel> ViewStatisticFillLevels { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ViewInventory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ViewInventory");

            entity.Property(e => e.Name)
                .HasMaxLength(16)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ViewStatisticBridgePerDay>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ViewStatisticBridgePerDay");

            entity.Property(e => e.BridgeNr)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.Hour)
                .HasMaxLength(1)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ViewStatisticBridgePerDayConsolidated>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ViewStatisticBridgePerDayConsolidated");

            entity.Property(e => e.BridgeNr)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.Hour)
                .HasMaxLength(1)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ViewStatisticBridgePerHour>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ViewStatisticBridgePerHour");

            entity.Property(e => e.BridgeNr)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.Hour)
                .HasMaxLength(6)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ViewStatisticBridgePerHourConsolidated>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ViewStatisticBridgePerHourConsolidated");

            entity.Property(e => e.BridgeNr)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.Hour)
                .HasMaxLength(6)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ViewStatisticFillLevel>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ViewStatisticFillLevel");

            entity.Property(e => e.Area)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.Day).HasMaxLength(4000);
        });
    }
}
