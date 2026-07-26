namespace SimulatorWebApp.Services;

public class HostedMessageBusService : IHostedService, IAsyncDisposable
{
    private readonly MessageBusService _messagebusService;

    public HostedMessageBusService(
        MessageBusService messageBusService)
    {
        _messagebusService = messageBusService;
    }

    public async Task StartAsync(CancellationToken token)
    {
        await _messagebusService.StartAsync();
    }

    public Task StopAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
