using Gudel.GLogWare.EFCore.Application;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Text;
using System.Timers;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string OP = string.Empty;
    public static string ServiceName => $"BridgeManager-{OP}";
    public static string ElementName => OP.Substring(2, 4);
    #endregion

    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    IDbContextFactory<GLogWareDbContext> _factory;
    private readonly DbLoggerService _dbLoggerService;
    #endregion

    #region Private members
    private string _mqttBrokerIp { get; set; } = "127.0.0.1";
    private int _mqttBrokerPort { get; set; } = 1883;
    private string _mqttBrokerRootTopic { get; set; } = string.Empty;
    private string _subscriptionTopic { get; set; } = string.Empty;
    private IManagedMqttClient? _mqttClient = null;
    private CancellationTokenSource? _cts;
    private System.Timers.Timer _watchdogWakeup = null!;
    private int _delayWakeup { get; set; } = 30000;
    private SemaphoreSlim _semaphoreLock = null!;
    #endregion

    public BridgeManager(
        ILogger<BridgeManager> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> factory,
        DbLoggerService dbLoggerService)
    {
        _logger = logger;
        _configuration = configuration;
        _factory = factory;
        _dbLoggerService = dbLoggerService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LoadConfiguration();

        _semaphoreLock = new SemaphoreSlim(1);

        await StartMqtt();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = TcpConnectLoopAsync(_cts.Token);

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    private void LoadConfiguration()
    {
        LoadConfiguration_Mqtt();
        LoadConfiguration_Plc();
    }

    private void LoadConfiguration_Mqtt()
    {
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
        _mqttClient = new MqttFactory().CreateManagedMqttClient();

        var mqttOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBrokerIp, _mqttBrokerPort)
                .WithClientId($"BridgeManager-{OP}")
                .WithCleanSession(false)
                .Build())
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e => {
            await OnMqttMessageReceived(e);
            await Task.CompletedTask;
        };

        _mqttClient.ConnectedAsync += async e => {
            _logger.LogInformation("Connected to MQTT broker.");
            await Task.CompletedTask;
        };

        _mqttClient.DisconnectedAsync += async e => {
            _logger.LogInformation("Disconnected from MQTT broker.");
            await Task.CompletedTask;
        };

        _subscriptionTopic = $"{_mqttBrokerRootTopic}/GantryBridges/{OP}/Manager/Incoming";
        _logger.LogInformation($"subscriptionTopic=[{_subscriptionTopic}]");

        var mqttSubscriptionTopics = new[] {
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

    public async Task OnMqttMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            _logger.LogInformation(payload);

            GLogWareMessage m = GLogWareMessage.DeSerialize(payload)!;
            switch (m.Identifier)
            {
                case GLogWareMessageIdentifiers.ToPlc:
                    PlcMessage pmTo = GLogWareMessage.DeSerialize<PlcMessage>(m.Data!.ToString()!)!;
                    await SendTelegram(pmTo);
                    break;
                case GLogWareMessageIdentifiers.FromPlc:
                    await Lock();
                    try
                    {
                        PlcMessage pmFrom = GLogWareMessage.DeSerialize<PlcMessage>(m.Data!.ToString()!)!;
                        await SimulatePlcTelegram(pmFrom);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing GLogWareMessageIdentifiers.FromPlc");
                    }
                    Unlock();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GLogWareMessage");
        }

        await Lock();
        try
        {
            await TryToStartNewOrder();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
        }
        Unlock();
    }

    public async Task SendToMqtt(string topic, GLogWareMessage m)
    {
        string payload = string.Empty;

        try
        {
            m.Sender = ServiceName;
            payload = m.Serialize();

            _logger.LogInformation($"topic=[{topic}]");
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

    private async void OnWatchdogWakeup(object source, ElapsedEventArgs e)
    {
        await SendWakeUp(_subscriptionTopic);
    }

    private async Task SendWakeUp(string topic)
    {
        GLogWareMessage gm = new GLogWareMessage();
        gm.Identifier = GLogWareMessageIdentifiers.WakeUp;
        await SendToMqtt(_subscriptionTopic, gm);
        RestartTimer(_watchdogWakeup);
    }

    private async Task Lock()
    {
        await _semaphoreLock.WaitAsync();

        if (_db != null)
        {
            _db.Dispose();
            _db = null;
        }
        _db = _factory.CreateDbContext();
    }

    private void Unlock()
    {
        _semaphoreLock.Release();
    }
}