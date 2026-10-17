namespace Gudel.GLogWare.Entities;

public class Route : BaseTracking
{
    public int Id { get; set; }
    public string DecisionPlace { get; set; } = null!;
    public string DestinationPlace { get; set; } = null!;
    public string NextPlace { get; set; } = null!;
    public string? Conditions { get; set; }
    public int Prio { get; set; }

    public Place DecisionPlaceRecord { get; set; } = null!;
    public Place DestinationPlaceRecord { get; set; } = null!;
    public Place NextPlaceRecord { get; set; } = null!;
}
