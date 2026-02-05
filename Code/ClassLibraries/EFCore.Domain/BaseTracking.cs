namespace Gudel.GLogWare.EFCore.Domain;

public abstract class BaseTracking
{
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
}
