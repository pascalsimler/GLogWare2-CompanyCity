namespace Gudel.GLogWare.EFCore.Domain;

public abstract class BaseTracking
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}
