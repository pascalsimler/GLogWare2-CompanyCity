using Gudel.GLogWare.Entities;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.Services.ConveyorManager;

public partial class ConveyorManager
{
    #region Private members

    #endregion

    private async Task<bool> ProcessWaitOnRouteJobs()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        var waitOnRouteJobs = _db.Jobs
            .Include(j => j.ActualPlaceRecord)
            .Where(j => j.Status == nameof(JobStatusIdentifiers.WAIT_ON_ROUTE))
            .OrderBy(j => j.ModifiedAt);

        foreach (Job job in waitOnRouteJobs)
        {
            _logger.LogInformation($"JobId=[{job.Jobid}]");
            _logger.LogInformation($"ActualPlace=[{job.ActualPlace}]");
            _logger.LogInformation($"ActualPlaceRecord.PlaceType=[{job.ActualPlaceRecord.PlaceType}]");
            _logger.LogInformation($"----------------------------------");
        }

        _logger.LogInformation(LogMessages.LeaveMethod);

        return false;
    }

    private async Task Process_STAT(STATConveyor stat)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        string json = GLogWareMessage.Serialize<STATConveyor>(stat);
        _logger.LogInformation($"stat=[\r\n{json}\r\n]");

      
        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task Process_ARIV(ARIV ariv)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        string json = GLogWareMessage.Serialize<ARIV>(ariv);
        _logger.LogInformation($"ariv=[\r\n{json}\r\n]");

     
        _logger.LogInformation(LogMessages.LeaveMethod);
    }
}