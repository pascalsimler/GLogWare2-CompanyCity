using Gudel.GLogWare.Infrastructure;
using Gudel.GLogWare.Interfaces;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Microsoft.EntityFrameworkCore;
using System.Timers;

namespace Gudel.GLogWare.Services.BridgeSimulator;

public partial class BridgeSimulator(
    ILogger<BridgeSimulator> logger,
    IConfiguration configuration,
    IMessageBus messageBus,
    IPlcDriver plcSimulatorDriver,
    IDbContextFactory<GLogWareDbContext> dbContextFactory
) : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string? OP = string.Empty;
    public static string ServiceName => $"BridgeSimulator-{OP}";
    #endregion

    #region Private members
    private readonly string _configPath = $"GantryBridges:{OP}";
    private string _subscriptionTopic = string.Empty;
    private System.Timers.Timer _watchdogWakeup = null!;
    private int _delayWakeup = 30000;
    private SemaphoreSlim _semaphoreLock = null!;
    private GLogWareDbContext _db = null!;
    #endregion

    #region Public methods
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.EnterMethod();

        _semaphoreLock = new SemaphoreSlim(1);

        LoadConfiguration();

        await messageBus.StartAsync();
        _ = StartPlcSimulatorDriverAsync(cancellationToken);

        _watchdogWakeup = new System.Timers.Timer(_delayWakeup);
        _watchdogWakeup.Elapsed += OnWatchdogWakeup!;
        _watchdogWakeup.AutoReset = true;
        _watchdogWakeup.Start();

        await Task.CompletedTask;

        logger.LeaveMethod();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.EnterMethod();

        await Task.CompletedTask;

        logger.LeaveMethod();
    }

    public async ValueTask DisposeAsync()
    {
        logger.EnterMethod();

        _watchdogWakeup?.Dispose();
        _semaphoreLock?.Dispose();
        if (_db != null)
        {
            await _db.DisposeAsync();
        }

        logger.LeaveMethod();

        GC.SuppressFinalize(this);
    }
    #endregion

    #region Private methods
    private void LoadConfiguration()
    {
        logger.EnterMethod();

        _subscriptionTopic = $"GantryBridges/{OP}/Simulation/Incoming";
        messageBus.MessageBusNotification += OnMessageBusNotification;
        messageBus.Init(ServiceName, [_subscriptionTopic]);

        if (int.TryParse(configuration[$"{_configPath}:DelayWakeup"], out int tmpDelayWakeup)) _delayWakeup = tmpDelayWakeup;
        logger.LogKeyValue("_delayWakeup", _delayWakeup);

        LoadGLogWareConfiguration();
        InitSimulation();

        logger.LeaveMethod();
    }

    private async void OnMessageBusNotification(object? sender, MessageBusNotificationEventArgs e)
    {
        logger.EnterMethod();
        logger.LogKeyValue("e.NotificationType", e.NotificationType);

        switch (e.NotificationType)
        {
            case MessageBusNotificationType.Connected:
                logger.LogInformation("Connected to message bus.");
                break;
            case MessageBusNotificationType.Disconnected:
                logger.LogInformation("Disconnected from message bus.");
                break;
            case MessageBusNotificationType.MessageReceived:
                logger.LogKeyValue("e.Topic", e.Topic);
                logger.LogKeyValue("e.Payload", e.Payload);
                await ProcessMessageBusPayload(e.Payload);
                break;
        }

        logger.LeaveMethod();
    }

    public async Task ProcessMessageBusPayload(string payload)
    {
        logger.EnterMethod();
        logger.LogKeyValue("payload", payload);

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
                    await plcSimulatorDriver.SendAsync(pm);
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
            logger.LogError(ex, "Error processing GLogWareMessage");
        }
        finally
        {
            Unlock();
        }

        logger.LeaveMethod();
    }

    public async Task SendGLogWareMessage(string topic, GLogWareMessage m)
    {
        logger.EnterMethod();
        logger.LogKeyValue("topic", topic);

        try
        {
            m.Sender = ServiceName;
            string payload = m.Serialize();
            logger.LogKeyValue("payload", $"\r\n{payload}\r\n");
            await messageBus.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception");
        }

        logger.LeaveMethod();
    }

    private async void OnWatchdogWakeup(object source, ElapsedEventArgs e)
    {
        logger.EnterMethod();

        await SendWakeUp(_subscriptionTopic);

        logger.LeaveMethod();
    }

    private async Task SendWakeUp(string topic)
    {
        logger.EnterMethod();

        GLogWareMessage gm = new()
        {
            Identifier = GLogWareMessageIdentifiers.WakeUp
        };
        await SendGLogWareMessage(topic, gm);

        logger.LeaveMethod();
    }

    private async Task Lock()
    {
        logger.EnterMethod();

        _watchdogWakeup.Stop();
        await _semaphoreLock.WaitAsync();
        if (_db != null)
        {
            _db.Dispose();
            _db = null!;
        }
        _db = await dbContextFactory.CreateDbContextAsync();

        logger.LeaveMethod();
    }

    private void Unlock()
    {
        logger.EnterMethod();

        _semaphoreLock.Release();
        _watchdogWakeup.Start();

        logger.LeaveMethod();
    }
    #endregion
}