namespace Gudel.GLogWare.UI.Entities;

public class User :BaseTracking
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsLdap { get; set; }
    public bool IsLocked { get; set; }
    public DateTime LastSuccessfulLoginAt { get; set; }
}
