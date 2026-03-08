using Gudel.GLogWare.EFCore.Application;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Text;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string OP = string.Empty;
    public static string ServiceName => $"BridgeManager-{OP}";
    public static string ElementName => OP.Substring(3, 4);
    #endregion

    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly GLogWareDbContext _db;
    private readonly DbLoggerService _dbLoggerService;
    #endregion

    #region Private members
    private string _mqttBrokerIp { get; set; } = "127.0.0.1";
    private int _mqttBrokerPort { get; set; } = 1883;
    private string _mqttBrokerRootTopic { get; set; } = string.Empty;
    private IManagedMqttClient? _mqttClient = null;
    private CancellationTokenSource? _cts;
    #endregion

    public BridgeManager(
        ILogger<BridgeManager> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> factory,
        DbLoggerService dbLoggerService)
    {
        _logger = logger;
        _configuration = configuration;
        _db = factory.CreateDbContext();
        _dbLoggerService = dbLoggerService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LoadConfiguration();

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
        _logger.LogInformation($"_mqttBrokerIp=[{_mqttBrokerIp}]");
        _logger.LogInformation($"_mqttBrokerPort=[{_mqttBrokerPort}]");
        _logger.LogInformation($"_mqttBrokerRootTopic=[{_mqttBrokerRootTopic}]");
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

        string subscriptionTopic = $"{_mqttBrokerRootTopic}/GantryBridges/{OP}/Manager/Incoming";
        _logger.LogInformation($"subscriptionTopic=[{subscriptionTopic}]");

        var mqttSubscriptionTopics = new[] {
            new MqttTopicFilterBuilder()
                .WithTopic(subscriptionTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build()
        };

        await _mqttClient.SubscribeAsync(mqttSubscriptionTopics);
        await _mqttClient.StartAsync(mqttOptions);
    }

    public async Task OnMqttMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        //_plcSendingReleased.WaitOne();
        //_plcSendingReleased.Reset();

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
                    PlcMessage pmFrom = GLogWareMessage.DeSerialize<PlcMessage>(m.Data!.ToString()!)!;
                    await SimulatePlcTelegram(pmFrom);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GLogWareMessage");
        }

        await TryToStartNewOrder();
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
}