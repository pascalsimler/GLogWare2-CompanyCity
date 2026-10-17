namespace Gudel.GLogWare.Entities;

public class RolePermission : BaseTracking
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = default!;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = default!;
}
