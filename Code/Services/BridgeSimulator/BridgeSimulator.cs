using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.MessageBus;
using Gudel.GLogWare.Messages;
using Gudel.GLogWare.PlcDriver;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.Services.BridgeSimulator;

public partial class BridgeSimulator : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string? OP = string.Empty;
    public static string ServiceName => $"BridgeSimulator-{OP}";
    #endregion

    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IMessageBus _messageBus;
    private readonly IPlcDriver _plcSimulatorDriver;
    private readonly IDbContextFactory<GLogWareDbContext> _dbContextFactory;
    private GLogWareDbContext _db = null!;
    #endregion

    #region Private members
    private string _subscriptionTopic { get; set; } = string.Empty;
    private SemaphoreSlim _semaphoreLock = null!;
    #endregion
   
    public BridgeSimulator(
        ILogger<BridgeSimulator> logger,
        IConfiguration configuration,
        IMessageBus messageBus,
        IPlcDriver plcSimulatorDriver,
        IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _messageBus = messageBus;
        _plcSimulatorDriver = plcSimulatorDriver;
        _dbContextFactory = dbContextFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        LoadConfiguration();

        await _messageBus.StartAsync();
        _ = StartPlcSimulatorDriverAsync(cancellationToken);

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await Task.CompletedTask;
    }

    private void LoadConfiguration()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _subscriptionTopic = $"GantryBridges/{OP}/Simulation/Incoming";
        _messageBus.MessageBusNotification += OnMessageBusNotification;
        _messageBus.LoadConfiguration(
            $"BridgeSimulator-{OP}",
            new string[] {
                _subscriptionTopic
            }
        );
        //    if (int.TryParse(_configuration[$"{path}:DelayWakeup"], out int tmpDelayWakeup)) _delayWakeup = tmpDelayWakeup;
        //    _logger.LogInformation($"_delayWakeup=[{_delayWakeup}]");

        LoadGLogWareConfiguration();
        InitSimulation();
    }

    private async void OnMessageBusNotification(object? sender, MessageBusNotificationEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"e.Notification§Type=[{e.NotificationType}]");

        switch (e.NotificationType)
        {
            case MessageBusNotificationType.Connected:
                _logger.LogInformation($"Connected to message bus.");
                break;
            case MessageBusNotificationType.Disconnected:
                _logger.LogInformation($"Disconnected from message bus.");
                break;
            case MessageBusNotificationType.MessageReceived:
                _logger.LogInformation($"e.Topic=[{e.Topic}]");
                _logger.LogInformation($"e.Payload=[{e.Payload}]");
                await ProcessMessageBusPayload(e.Payload);
                break;
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    public async Task ProcessMessageBusPayload(string payload)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"payload=[{payload}]");

        try
        {
            GLogWareMessage m = GLogWareMessage.DeSerialize(payload)!;
            switch (m.Identifier)
            {
                case GLogWareMessageIdentifiers.WakeUp:
                    break;
                case GLogWareMessageIdentifiers.ToGLogWare:
                    PlcMessage pm = GLogWareMessage.DeSerialize<PlcMessage>(m.Data!.ToString()!)!;
                    await ProcessPlcMessage(pm);
                    await _plcSimulatorDriver.SendAsync(pm);
                    break;
                case GLogWareMessageIdentifiers.FromGLogWare:
                    break;
                case GLogWareMessageIdentifiers.Configuration:
                    BridgeConfiguration bc = GLogWareMessage.DeSerialize<BridgeConfiguration>(m.Data!.ToString()!)!;
                    SetBridgeConfiguration(bc);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GLogWareMessage");
        }
    }

    public async Task SendGLogWareMessage(string topic, GLogWareMessage m)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"topic=[{topic}]");

        try
        {
            m.Sender = ServiceName;
            string payload = m.Serialize();
            _logger.LogInformation($"payload=[\r\n{payload}\r\n]");
            await _messageBus.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
        }

    }

    private async Task Lock()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await _semaphoreLock.WaitAsync();
        if (_db != null)
        {
            _db.Dispose();
            _db = null!;
        }
        _db = _dbContextFactory.CreateDbContext();

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private void Unlock()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _semaphoreLock.Release();

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

}