namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class JobStatus : BaseTracking, ISeedData<JobStatus>
{
    public string? Name { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public static IEnumerable<JobStatus> SeedData()
    {
        return new List<JobStatus>() {
            new JobStatus { 
                Name = JobStatusNames.OK_BRIDGE.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.OK_BRIDGE.ToString()}",
                Description = "Bridge is ready to receive a new pick order"
            },
            new JobStatus {
                Name = JobStatusNames.BRIDGE_LOAD.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.BRIDGE_LOAD.ToString()}",
                Description = "Bridge is currently processing a pick order"
            },
            new JobStatus {
                Name = JobStatusNames.BRIDGE_LOAD_END.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.BRIDGE_LOAD_END.ToString()}",
                Description = "Pick order has been processed"
            },
            new JobStatus {
                Name = JobStatusNames.OK_BRIDGE_UNLOAD.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.OK_BRIDGE_UNLOAD.ToString()}",
                Description = "Bridge is ready to receive a new drop order"
            },
            new JobStatus {
                Name = JobStatusNames.BRIDGE_UNLOAD.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.BRIDGE_UNLOAD.ToString()}",
                Description = "Bridge is currently processing a drop order"
            },
            new JobStatus {
                Name = JobStatusNames.BRIDGE_UNLOAD_END.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.BRIDGE_UNLOAD_END.ToString()}",
                Description = "Drop order has been processed"
            },
            new JobStatus {
                Name = JobStatusNames.WAIT_ON_JOBMANAGER.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.WAIT_ON_JOBMANAGER.ToString()}",
                Description = "New target needs to be calculated by the Job Manager"
            },
            new JobStatus {
                Name = JobStatusNames.WAIT_ON_ROUTE.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.WAIT_ON_ROUTE.ToString()}",
                Description = "Conveyor is waiting for a new order"
            },
            new JobStatus {
                Name = JobStatusNames.CONVEYOR_MOVE.ToString(),
                TranslationKey = $"{typeof(JobStatus).Name}.{JobStatusNames.CONVEYOR_MOVE.ToString()}",
                Description = "Job is moving on the conveyor"
            },
        };
    }
}