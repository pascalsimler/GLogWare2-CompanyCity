using Gudel.GLogWare.Shared;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Text;

namespace Gudel.GLogWare.BridgeSimulator;

public partial class BridgeSimulator : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string? OP = string.Empty;
    public static string ServiceName = string.Empty;
    #endregion

    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    #endregion

    #region Private members
    //MQTT
    private string _mqttBrokerIp { get; set; } = "127.0.0.1";
    private int _mqttBrokerPort { get; set; } = 1883;
    private string _mqttBrokerRootTopic { get; set; } = string.Empty;
    private string _subscriptionTopic { get; set; } = string.Empty;
    private IManagedMqttClient? _mqttClient = null;

    // Miscellaneous
    private CancellationTokenSource? _cts;
    #endregion

    public BridgeSimulator(
        ILogger<BridgeSimulator> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LoadConfiguration();

        await InitSimulation();

        await StartMqtt();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = TcpAcceptLoopAsync(_cts.Token);

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
        LoadConfigurationMqtt();
        LoadConfigurationGLogWare();
    }

    private void LoadConfigurationMqtt()
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
                .WithClientId($"BridgeSimulator-{OP}")
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

        _subscriptionTopic = $"{_mqttBrokerRootTopic}/GantryBridges/{OP}/Simulation/Incoming";
        _logger.LogInformation($"subscriptionTopic=[{_subscriptionTopic}]");

        var mqttSubscriptionTopics = new[] {
            new MqttTopicFilterBuilder()
                .WithTopic(_subscriptionTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build()
        };

        await _mqttClient.SubscribeAsync(mqttSubscriptionTopics);
        await _mqttClient.StartAsync(mqttOptions);
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
                case GLogWareMessageIdentifiers.WakeUp:
                    break;
                case GLogWareMessageIdentifiers.ToGLogWare:
                    PlcMessage pm = GLogWareMessage.DeSerialize<PlcMessage>(m.Data!.ToString()!)!;
                    await SendTelegram(pm);
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

    public async Task SendGLogWareMessageToMqtt(string topic, GLogWareMessage m)
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