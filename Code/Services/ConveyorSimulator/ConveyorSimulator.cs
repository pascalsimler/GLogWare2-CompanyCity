using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Text;

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
    private readonly IPlcDriver _plcSimulatorDriver;
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
    private SemaphoreSlim _semaphoreLock = null!;
    #endregion
   
    public ConveyorSimulator(
        ILogger<ConveyorSimulator> logger,
        IConfiguration configuration,
        IPlcDriver plcSimulatorDriver,
       IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _plcSimulatorDriver = plcSimulatorDriver;
        _dbContextFactory = dbContextFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        LoadConfiguration();

        await StartMqtt();
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

        LoadMqttConfiguration();
        LoadGLogWareConfiguration();
        InitSimulation();
    }

    private void LoadMqttConfiguration()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

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
        _logger.LogInformation(LogMessages.EnterMethod);

        _mqttClient = new MqttFactory().CreateManagedMqttClient();

        var mqttOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBrokerIp, _mqttBrokerPort)
                .WithClientId($"ConveyorSimulator-{OP}")
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

        _subscriptionTopic = $"{_mqttBrokerRootTopic}/Conveyors/{OP}/Simulation/Incoming";
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
        _logger.LogInformation(LogMessages.EnterMethod);

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

    public async Task SendGLogWareMessageToMqtt(string topic, GLogWareMessage m)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"topic=[{topic}]");

        try
        {
            m.Sender = ServiceName;
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