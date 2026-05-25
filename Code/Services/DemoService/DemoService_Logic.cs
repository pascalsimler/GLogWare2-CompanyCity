namespace Gudel.GLogWare.Services.DemoService;

public partial class DemoService
{
    private async Task DoWork()
    {
        using var _ = _logger.LogMethodScope();

        //try
        //{
        //    string logMsg = $"topic=[{topic}], message=[{message}]";
        //    _logger.LogInformation(logMsg);
        //    Protocol protocol = new Protocol();
        //    protocol.Message = logMsg;
        //    //await _dbLoggerService.WriteProtocolAsync(protocol);
        //    await Task.Delay(5000);
        //    await SendToMqtt($"{topic}-Response", message);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex.Message);
        //}
    }
}