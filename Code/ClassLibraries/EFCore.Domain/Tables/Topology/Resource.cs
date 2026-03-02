namespace Gudel.GLogWare.EFCore.Domain;

public class Resource : BaseTracking
{
    public string Name { get; set; } = null!;
    public string Mode { get; set; } = null!;
    public bool? Parked { get; set; }
    public bool? InfeedEnabled { get; set; }
    public bool? OutfeedEnabled { get; set; }
    public bool? RelocationEnabled { get; set; }

    public ResourceMode ResourceModeRecord { get; set; } = null!;
}
