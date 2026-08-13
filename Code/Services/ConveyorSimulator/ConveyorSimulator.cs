using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.MessageBus;
using Gudel.GLogWare.Messages;
using Gudel.GLogWare.PlcDriver;
using Microsoft.EntityFrameworkCore;
using System.Timers;

namespace Gudel.GLogWare.Services.ConveyorSimulator;

public partial class ConveyorSimulator : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string? OP = string.Empty;
    public static string ServiceName => $"ConveyorSimulator-{OP}";
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
    private string _configPath => $"Conveyors:{OP}";
    private string _subscriptionTopic { get; set; } = string.Empty;
    private System.Timers.Timer _watchdogWakeup = null!;
    private int _delayWakeup { get; set; } = 30000;
    private SemaphoreSlim _semaphoreLock = null!;
    #endregion

    #region Constructors
    public ConveyorSimulator(
        ILogger<ConveyorSimulator> logger,
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
    #endregion

    #region Public methods
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _semaphoreLock = new SemaphoreSlim(1);

        LoadConfiguration();

        await _messageBus.StartAsync();
        _ = StartPlcSimulatorDriverAsync(cancellationToken);

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _watchdogWakeup?.Dispose();
        _semaphoreLock?.Dispose();
        if (_db != null)
        {
            await _db.DisposeAsync();
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }
    #endregion

    #region Private methods
    private void LoadConfiguration()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _subscriptionTopic = $"Conveyors/{OP}/Simulation/Incoming";
        _messageBus.MessageBusNotification += OnMessageBusNotification;
        _messageBus.Init(
            ServiceName,
            new string[] {
                _subscriptionTopic
            }
        );
        if (int.TryParse(_configuration[$"{_configPath}:DelayWakeup"], out int tmpDelayWakeup)) _delayWakeup = tmpDelayWakeup;
        _logger.LogInformation($"_delayWakeup=[{_delayWakeup}]");

        LoadGLogWareConfiguration();
        InitSimulation();

        _logger.LogInformation(LogMessages.LeaveMethod);
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

        await Lock();
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
        finally
        {
            Unlock();
            ResetTimer(_watchdogWakeup);
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
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

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async void OnWatchdogWakeup(object source, ElapsedEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await SendWakeUp(_subscriptionTopic);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task SendWakeUp(string topic)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        GLogWareMessage gm = new GLogWareMessage();
        gm.Identifier = GLogWareMessageIdentifiers.WakeUp;
        await SendGLogWareMessage(_subscriptionTopic, gm);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task Lock()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _watchdogWakeup.Stop();
        await _semaphoreLock.WaitAsync();
        if (_db != null)
        {
            _db.Dispose();
            _db = null!;
        }
        _db = await _dbContextFactory.CreateDbContextAsync();

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private void Unlock()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _semaphoreLock.Release();
        _watchdogWakeup.Start();

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private void ResetTimer(System.Timers.Timer timer)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        timer.Stop();
        timer.Start();

        _logger.LogInformation(LogMessages.LeaveMethod);
    }
    #endregion
}