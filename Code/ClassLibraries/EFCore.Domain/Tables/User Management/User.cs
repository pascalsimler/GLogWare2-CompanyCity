namespace Gudel.GLogWare.EFCore.Domain;

public class User :BaseTracking
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public bool IsLdap { get; set; }
}
