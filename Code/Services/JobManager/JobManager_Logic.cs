using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.Services.JobManager;

public partial class JobManager
{
    private async Task DoWork()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await ProcessWaitingJobs();
        await CreateJobsForOutputOrders();
        
        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task ProcessWaitingJobs()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        var waitingJobs = _db.Jobs
            .Include(j => j.ActualPlaceRecord)
            .Where(j => j.Status == nameof(JobStatusIdentifiers.WAIT_ON_JOBMANAGER))
            .OrderBy(j => j.ModifiedAt);

        foreach (Job job in waitingJobs)
        {
            _logger.LogInformation($"JobId=[{job.Jobid}]");
            _logger.LogInformation($"ActualPlace=[{job.ActualPlace}]");
            _logger.LogInformation($"ActualPlaceRecord.PlaceType=[{job.ActualPlaceRecord.PlaceType}]");
            switch (job.ActualPlaceRecord.PlaceType)
            {
                case nameof(PlaceTypeIdentifiers.GANTRY_PICK):
                    _logger.LogInformation($"CRUCHOTAGE !!!");
                    break;

                default:
                    break;
            }
            _logger.LogInformation($"----------------------------------");
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task CreateJobsForOutputOrders()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }
}
