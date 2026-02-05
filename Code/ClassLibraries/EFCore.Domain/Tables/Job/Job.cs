namespace Gudel.GLogWare.EFCore.Domain;

public class Job : BaseTracking
{
    public string JobId { get; set; } = null!;
    public string JobStatus { get; set; } = null!;

    public JobStatus JobStatusRecord { get; set; } = null!;
}