using Gudel.GLogWare.Infrastructure;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.MessageBus;
using Gudel.GLogWare.Messages;
using Microsoft.EntityFrameworkCore;
using System.Timers;

namespace Gudel.GLogWare.Services.DemoService;

public partial class DemoService : IHostedService, IAsyncDisposable
{
    #region Public member
    public static string ServiceName { get; set; } = string.Empty;
    #endregion

    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IMessageBus _messageBus;
    private readonly IDbContextFactory<GLogWareDbContext> _dbContextFactory;
    private GLogWareDbContext _db = null!;
    #endregion

    #region Private members
    private string _configPath => $"{ServiceName}";
    private string _subscriptionTopic { get; set; } = string.Empty;
    private System.Timers.Timer _watchdogWakeup = null!;
    private int _delayWakeup { get; set; } = 30000;
    private SemaphoreSlim _semaphoreLock = null!;
    #endregion

    #region Constructors
    public DemoService(
        ILogger<DemoService> logger,
        IConfiguration configuration,
        IMessageBus messageBus,
        IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _messageBus = messageBus;
        _dbContextFactory = dbContextFactory;
    }
    #endregion

    #region Public methods
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        _semaphoreLock = new SemaphoreSlim(1);

        LoadConfiguration();

        await _messageBus.StartAsync();

        _watchdogWakeup = new System.Timers.Timer(_delayWakeup);
        _watchdogWakeup.Elapsed += OnWatchdogWakeup!;
        _watchdogWakeup.AutoReset = true;
        _watchdogWakeup.Start();

        await Task.CompletedTask;

        _logger.LeaveMethod();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        await Task.CompletedTask;

        _logger.LeaveMethod();
    }

    public async ValueTask DisposeAsync()
    {
        _logger.EnterMethod();

        _watchdogWakeup?.Dispose();
        _semaphoreLock?.Dispose();
        if (_db != null)
        {
            await _db.DisposeAsync();
        }

        await Task.CompletedTask;

        _logger.LeaveMethod();
    }
    #endregion

    #region Private methods
    private void LoadConfiguration()
    {
        _logger.EnterMethod();

        _subscriptionTopic = $"{ServiceName}/Incoming"; ;
        _messageBus.MessageBusNotification += OnMessageBusNotification;
        _messageBus.Init(
            ServiceName,
            new string[] {
                _subscriptionTopic
            }
        );
        if (int.TryParse(_configuration[$"{_configPath}:DelayWakeup"], out int tmpDelayWakeup)) _delayWakeup = tmpDelayWakeup;
        _logger.LogKeyValue($"_delayWakeup", _delayWakeup);

        _logger.LeaveMethod();
    }

    private async void OnMessageBusNotification(object? sender, MessageBusNotificationEventArgs e)
    {
        _logger.EnterMethod();
        _logger.LogKeyValue($"e.NotificationType", e.NotificationType);

        switch (e.NotificationType)
        {
            case MessageBusNotificationType.Connected:
                _logger.LogInformation("Connected to message bus.");
                break;
            case MessageBusNotificationType.Disconnected:
                _logger.LogInformation("Disconnected from message bus.");
                break;
            case MessageBusNotificationType.MessageReceived:
                _logger.LogKeyValue($"e.Topic", e.Topic);
                _logger.LogKeyValue($"e.Payload", e.Payload);
                await ProcessMessageBusPayload(e.Payload);
                break;
        }

        _logger.LeaveMethod();
    }

    private async Task ProcessMessageBusPayload(string payload)
    {
        _logger.EnterMethod();

        await Lock();
        try
        {
            GLogWareMessage m = GLogWareMessage.DeSerialize(payload)!;
            switch (m.Identifier)
            {
                case GLogWareMessageIdentifiers.WakeUp:
                    //await DoWork();
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

        _logger.LeaveMethod();
    }

    private async Task SendGLogWareMessage(string topic, GLogWareMessage m)
    {
        _logger.EnterMethod();
        _logger.LogKeyValue($"topic", topic);

        try
        {
            string payload = m.Serialize();
            _logger.LogKeyValue("payload", $"\r\n{payload}\r\n");
            await _messageBus.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
        }

        _logger.LeaveMethod();
    }
    private async void OnWatchdogWakeup(object source, ElapsedEventArgs e)
    {
        _logger.EnterMethod();

        await SendWakeUp(_subscriptionTopic, "Wakeup timer ellapsed");

        _logger.LeaveMethod();
    }

    private async Task SendWakeUp(string topic, string message)
    {
        _logger.EnterMethod();
        _logger.LogKeyValue($"topic", topic);
        _logger.LogKeyValue($"message", message);

        GLogWareMessage gm = new GLogWareMessage();
        gm.Identifier = GLogWareMessageIdentifiers.WakeUp;
        gm.Data = message;
        await SendGLogWareMessage(_subscriptionTopic, gm);

        _logger.LeaveMethod();
    }

    private async Task Lock()
    {
        _logger.EnterMethod();

        _watchdogWakeup.Stop();
        await _semaphoreLock.WaitAsync();
        if (_db != null)
        {
            _db.Dispose();
            _db = null!;
        }
        _db = await _dbContextFactory.CreateDbContextAsync();

        _logger.LeaveMethod();
    }

    private void Unlock()
    {
        _logger.EnterMethod();

        _semaphoreLock.Release();
        _watchdogWakeup.Start();

        _logger.LeaveMethod();
    }

    private void ResetTimer(System.Timers.Timer timer)
    {
        _logger.EnterMethod();

        timer.Stop();
        timer.Start();

        _logger.LeaveMethod();
    }
    #endregion
}