using Gudel.GLogWare.Entities;
using Gudel.GLogWare.Logging;

namespace Gudel.GLogWare.Services.DemoService;

public partial class DemoService
{
    private int _counter = 0;

    private async Task DoWork()
    {
        _logger.EnterMethod();

        try
        {
            _counter++;
            string logMsg = $"Counter=[{_counter}]";
            _logger.LogInformation(logMsg);
            Protocol protocol = new Protocol();
            protocol.Message = logMsg;
            _db.Protocols.Add(protocol);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Start long procesing task");
            await Task.Delay(5000);
            _logger.LogInformation("Finished long processing task");
            //await SendToMqtt($"{topic}-Response", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        _logger.LeaveMethod();
    }
}