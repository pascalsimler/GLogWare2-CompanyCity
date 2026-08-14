namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public class JobStatus : BaseTracking, ISeedData<JobStatus>
{
    public string Identifier { get; set; } = null!;
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public static IEnumerable<JobStatus> SeedData()
    {
        return new List<JobStatus>() {
            new JobStatus { 
                Identifier = nameof(JobStatusIdentifiers.OK_BRIDGE),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.OK_BRIDGE)}",
                Description = "Bridge is ready to receive a new pick order"
            },
            new JobStatus {
                Identifier = nameof(JobStatusIdentifiers.BRIDGE_LOAD),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.BRIDGE_LOAD)}",
                Description = "Bridge is currently processing a pick order"
            },
            new JobStatus {
                Identifier = nameof(JobStatusIdentifiers.BRIDGE_LOAD_END),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.BRIDGE_LOAD_END)}",
                Description = "Pick order has been processed"
            },
            new JobStatus {
                Identifier = nameof(JobStatusIdentifiers.OK_BRIDGE_UNLOAD),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.OK_BRIDGE_UNLOAD)}",
                Description = "Bridge is ready to receive a new drop order"
            },
            new JobStatus {
                Identifier = nameof(JobStatusIdentifiers.BRIDGE_UNLOAD),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.BRIDGE_UNLOAD)}",
                Description = "Bridge is currently processing a drop order"
            },
            new JobStatus {
                Identifier = nameof(JobStatusIdentifiers.BRIDGE_UNLOAD_END),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.BRIDGE_UNLOAD_END)}",
                Description = "Drop order has been processed"
            },
            new JobStatus {
                Identifier = nameof(JobStatusIdentifiers.WAIT_ON_JOBMANAGER),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.WAIT_ON_JOBMANAGER)}",
                Description = "New target needs to be calculated by the Job Manager"
            },
            new JobStatus {
                Identifier = nameof(JobStatusIdentifiers.WAIT_ON_ROUTE),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.WAIT_ON_ROUTE)}",
                Description = "Conveyor is waiting for a new order"
            },
            new JobStatus {
                Identifier = nameof(JobStatusIdentifiers.CONVEYOR_MOVE),
                TranslationKey = $"{nameof(JobStatus)}.{nameof(JobStatusIdentifiers.CONVEYOR_MOVE)}",
                Description = "Job is moving on the conveyor"
            },
        };
    }
}