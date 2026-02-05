namespace Gudel.GLogWare.DemoService;

public class Worker : IHostedService, IAsyncDisposable
{
    #region Private members
    private ILogger<Worker> _logger;
    private IConfiguration _configuration;
    #endregion

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting {Service} ...", nameof(Worker));

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping {Service} ...", nameof(Worker));

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }
}