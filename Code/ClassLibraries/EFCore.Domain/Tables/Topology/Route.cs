namespace Gudel.GLogWare.EFCore.Domain;

public class Route : BaseTracking
{
    public int Id { get; set; }
    public string? DecisionPos { get; set; }
    public string? DestinationPos { get; set; }
    public string? NextPos { get; set; }
    public int Prio { get; set; }
}
