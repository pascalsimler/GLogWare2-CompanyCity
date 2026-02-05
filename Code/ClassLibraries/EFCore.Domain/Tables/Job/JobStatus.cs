namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class JobStatus : BaseTracking, ISeedData<JobStatus>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public static IEnumerable<JobStatus> SeedData()
    {
        return new List<JobStatus>() {
            new JobStatus { Name = JobStatusConstants.OkBridge, Description = "Bridge is ready to receive an ORDS"},
            new JobStatus { Name = JobStatusConstants.BridgeLoad, Description = "Bridge is currently processing an ORDS" },
            new JobStatus { Name = JobStatusConstants.BridgeLoadEnd, Description = "Bridge has issued a COMP" },
            new JobStatus { Name = JobStatusConstants.WaitOnJobManager, Description = "New tarthet needs to be calculated by the Job Manager" },
            new JobStatus { Name = JobStatusConstants.WaitOnRoute, Description = "Conveyor is waiting for a new TARG" },
            new JobStatus { Name = JobStatusConstants.ConveyorMove, Description = "Job is moving on the conveyor" },
        };
    }
}