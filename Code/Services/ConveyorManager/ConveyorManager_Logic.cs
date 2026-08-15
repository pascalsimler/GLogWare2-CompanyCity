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
        logger.EnterMethod();

        var waitOnRouteJobs = _db.Jobs
            .Include(j => j.ActualPlaceRecord)
            .Where(j => j.Status == nameof(JobStatusIdentifiers.WAIT_ON_ROUTE))
            .OrderBy(j => j.ModifiedAt);

        foreach (Job job in waitOnRouteJobs)
        {
            logger.LogKeyValue("JobId", job.Jobid);
            logger.LogKeyValue("ActualPlace", job.ActualPlace);
            logger.LogKeyValue("ActualPlaceRecord.PlaceType", job.ActualPlaceRecord.PlaceType);
            logger.LogInformation($"----------------------------------");
        }

        logger.LeaveMethod();

        return false;
    }

    private async Task Process_STAT(STATConveyor stat)
    {
        logger.EnterMethod();

        string json = GLogWareMessage.Serialize<STATConveyor>(stat);
        logger.LogKeyValue("stat", $"\r\n{json}\r\n");

        logger.LeaveMethod();
    }

    private async Task Process_ARIV(ARIV ariv)
    {
        logger.EnterMethod();

        string json = GLogWareMessage.Serialize<ARIV>(ariv);
        logger.LogKeyValue($"ariv", $"\r\n{json}\r\n");

     
        logger.LeaveMethod();
    }
}