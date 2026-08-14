namespace Gudel.GLogWare.Entities;

public class Job : BaseTracking
{
    public string Jobid { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;

    public string? Bridge { get; set; } = null!;
    public string? SourcePlace { get; set; } = null!;
    public string? DestinationPlace { get; set; } = null!;
    public string? ActualPlace { get; set; } = null!;
    public string? NextPlace { get; set; } = null!;

    public string? Information { get; set; } = null!;

    public JobType JobTypeRecord { get; set; } = null!;
    public JobStatus JobStatusRecord { get; set; } = null!;
    public Place SourcePlaceRecord { get; set; } = null!;
    public Place DestinationPlaceRecord { get; set; } = null!;
    public Place ActualPlaceRecord { get; set; } = null!;
    public Place NextPlaceRecord { get; set; } = null!;
}