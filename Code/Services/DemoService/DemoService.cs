using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System.Text;
using System.Timers;

namespace Gudel.GLogWare.Services.DemoService;

public partial class DemoService : IHostedService, IAsyncDisposable
{
    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<GLogWareDbContext> _dbContextFactory;
    private GLogWareDbContext _db = null!;
    #endregion

    #region Mqtt parameters
    private string _mqttBrokerIp { get; set; } = "127.0.0.1";
    private int _mqttBrokerPort { get; set; } = 1883;
    private string _mqttBrokerRootTopic { get; set; } = string.Empty;
    private string _subscriptionTopic { get; set; } = string.Empty;
    #endregion

    #region Private members
    private IManagedMqttClient? _mqttClient = null;
    private System.Timers.Timer _watchdogWakeup = null!;
    private int _delayWakeup { get; set; } = 30000;
    private SemaphoreSlim _semaphoreLock = null!;
    #endregion

    #region Constructor
    public DemoService(
        ILogger<DemoService> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
    }
    #endregion

    private void LoadConfiguration()
    {
        using var _ = _logger.LogMethodScope();

        string path = "MQTTBroker";
        _mqttBrokerIp = _configuration[$"{path}:Ip"] ?? _mqttBrokerIp;
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpMqttBrokerPort)) _mqttBrokerPort = tmpMqttBrokerPort;
        _mqttBrokerRootTopic = _configuration[$"{path}:RootTopic"] ?? _mqttBrokerRootTopic;
        if (int.TryParse(_configuration[$"{path}:DelayWakeup"], out int tmpDelayWakeup)) _delayWakeup = tmpDelayWakeup;

        _logger.LogInformation($"_mqttBrokerIp=[{_mqttBrokerIp}]");
        _logger.LogInformation($"_mqttBrokerPort=[{_mqttBrokerPort}]");
        _logger.LogInformation($"_mqttBrokerRootTopic=[{_mqttBrokerRootTopic}]");
        _logger.LogInformation($"_delayWakeup=[{_delayWakeup}]");
    }

    private async Task StartMqtt()
    {
        using var _ = _logger.LogMethodScope();

        _mqttClient = new MqttFactory().CreateManagedMqttClient();

        ManagedMqttClientOptions mqttOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBrokerIp, _mqttBrokerPort)
                .WithClientId("DemoService")
                .WithCleanSession(false)
                .Build())
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e => {
            await OnMqttMessageReceived(e);
            await Task.CompletedTask;
        };

        _mqttClient.ConnectedAsync += async e => {
            _logger.LogInformation($"Connected to MQTT broker.");
            await Task.CompletedTask;
        };

        _mqttClient.DisconnectedAsync += async e => {
            _logger.LogInformation($"Disconnected from MQTT broker.");
            await Task.CompletedTask;
        };

        MqttTopicFilter[] mqttSubscriptionTopics = new[] {
            new MqttTopicFilterBuilder()
                .WithTopic(_subscriptionTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build()
        };

        await _mqttClient.SubscribeAsync(mqttSubscriptionTopics);
        await _mqttClient.StartAsync(mqttOptions);

        _watchdogWakeup = new System.Timers.Timer(_delayWakeup);
        _watchdogWakeup.Elapsed += OnWatchdogWakeup!;
        _watchdogWakeup.AutoReset = true;
        _watchdogWakeup.Start();
    }

    private async Task OnMqttMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        using var _ = _logger.LogMethodScope();

        await Lock();
        try
        {
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            _logger.LogInformation(payload);

            GLogWareMessage m = GLogWareMessage.DeSerialize(payload)!;
            switch (m.Identifier)
            {
                case GLogWareMessageIdentifiers.WakeUp:
                    await DoWork();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GLogWareMessage");
        }
        Unlock();
    }

    private async Task SendGLogWareMessageToMqtt(string topic, GLogWareMessage m)
    {
        using var _ = _logger.LogMethodScope();
        _logger.LogInformation($"topic=[{topic}]");

        try
        {
            string payload = m.Serialize();
            _logger.LogInformation($"payload=[\r\n{payload}\r\n]");

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();
            await _mqttClient!.EnqueueAsync(mqttMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var _ = _logger.LogMethodScope();

        ////#region Timer
        ////_timer = new System.Timers.Timer(2000);
        ////_timer.Elapsed += OnTimer;
        ////_timer.AutoReset = true;
        ////_timer.Enabled = true;
        ////#endregion

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var _ = _logger.LogMethodScope();

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    { 
        using var _ = _logger.LogMethodScope();
        
        await Task.CompletedTask;
    }

    private async void OnWatchdogWakeup(object source, ElapsedEventArgs e)
    {
        using var _ = _logger.LogMethodScope();

        await SendWakeUp(_subscriptionTopic);
    }

    private async Task SendWakeUp(string topic)
    {
        using var _ = _logger.LogMethodScope();

        GLogWareMessage gm = new GLogWareMessage();
        gm.Identifier = GLogWareMessageIdentifiers.WakeUp;
        await SendGLogWareMessageToMqtt(_subscriptionTopic, gm);
    }

    private async Task Lock()
    {
        using var _ = _logger.LogMethodScope();

        await _semaphoreLock.WaitAsync();

        if (_db != null)
        {
            _db.Dispose();
            _db = null!;
        }
        _db = _dbContextFactory.CreateDbContext();
    }

    private void Unlock()
    {
        using var _ = _logger.LogMethodScope();

        _semaphoreLock.Release();
    }
}
