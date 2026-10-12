using System;
using System.Collections.Generic;
using Gudel.GLogWare.Infrastructure.Views;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.Infrastructure;

public partial class GLogWareDbContext
{
    public virtual DbSet<VInventory> VInventories { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VInventory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vInventory");

            entity.Property(e => e.Name)
                .HasMaxLength(16)
                .IsUnicode(false);
        });
    }
}
