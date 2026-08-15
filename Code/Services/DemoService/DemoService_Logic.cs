using Gudel.GLogWare.Entities;
using Gudel.GLogWare.Logging;

namespace Gudel.GLogWare.Services.DemoService;

public partial class DemoService
{
    private int _counter = 0;

    private async Task DoWork()
    {
        logger.EnterMethod();

        try
        {
            _counter++;
            string logMsg = $"Counter=[{_counter}]";
            logger.LogInformation(logMsg);
            Protocol protocol = new()
            {
                Message = logMsg
            };
            _db.Protocols.Add(protocol);
            await _db.SaveChangesAsync();
            logger.LogInformation("Start long procesing task");
            await Task.Delay(5000);
            logger.LogInformation("Finished long processing task");
            //await messageBus.PublishAsync($"{topic}-Response", message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in DoWork");
        }

        logger.LeaveMethod();
    }
}