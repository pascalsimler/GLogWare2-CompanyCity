namespace Gudel.GLogWare.EFCore.Domain;

public class Element : BaseTracking
{
    public string Name { get; set; } = null!;
    public bool? InfeedEnabled { get; set; }
    public bool? OutfeedEnabled { get; set; }
    public bool? RelocationEnabled { get; set; }
}
