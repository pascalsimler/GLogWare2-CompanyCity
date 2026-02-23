namespace Gudel.GLogWare.BridgeDriver;

public class Worker : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string? OP = string.Empty;
    public static string ServiceName = string.Empty;
    #endregion

    #region Private members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly PlcCommunication _plcCommunication;
    #endregion

    public Worker(
         ILogger<Worker> logger,
         IConfiguration configuration,
         PlcCommunication plcCommunication)
    {
        _logger = logger;
        _configuration = configuration;
        _plcCommunication = plcCommunication;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting ...");
        await _plcCommunication.StartAsync(cancellationToken);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Stopping ...");
        await _plcCommunication.StopAsync(cancellationToken);
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }
}
