using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.HasKey(e => new { e.RoleId, e.PermissionId });

        //entity.HasOne(e => e.Role)
        //      .WithMany(r => r.RolePermissions)
        //      .HasForeignKey(e => e.RoleId)
        //      .OnDelete(DeleteBehavior.Cascade)
        //      .HasConstraintName("FK_RolePermission_Role");

        //entity.HasOne(e => e.Permission)
        //      .WithMany(p => p.RolePermissions)
        //      .HasForeignKey(e => e.PermissionId)
        //      .OnDelete(DeleteBehavior.Cascade)
        //      .HasConstraintName("FK_RolePermission_Permission");
    }
}
