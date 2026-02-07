using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System.Text;

namespace Gudel.GLogWare.DemoService;

public class Worker : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string ServiceName = string.Empty;
    #endregion

    #region Private members
    private ILogger<Worker> _logger;
    private IConfiguration _configuration;
    private IDbContextFactory<GLogWareDbContext> _dbFactory;
    private GLogWareDbContext? _dbCtx;
    private DbLogger _dbLogger;
    private IManagedMqttClient? _mqttClient;
    private string? _subscriptionTopic;
    #endregion

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> dbFactory,
        DbLogger dbLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _dbFactory = dbFactory;
        _dbLogger = dbLogger;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting {ServiceName} ...");

        string mqttBrokerConfigPath = "MQTTBroker";
        string mqttBrokerHostname = _configuration[$"{mqttBrokerConfigPath}:Hostname"]!;
        int mqttBrokerPort = int.Parse(_configuration[$"{mqttBrokerConfigPath}:Port"]!);
        string mqttBrokerRootTopic = _configuration[$"{mqttBrokerConfigPath}:RootTopic"]!;
        _subscriptionTopic = $"{mqttBrokerRootTopic}/{ServiceName}";

        _logger.LogInformation($"mqttBrokerHostName=[{mqttBrokerHostname}]");
        _logger.LogInformation($"mqttBrokerPort=[{mqttBrokerPort}]");
        _logger.LogInformation($"mqttBrokerRootTopic=[{mqttBrokerRootTopic}]");
        _logger.LogInformation($"subscriptionTopic=[{_subscriptionTopic}]");

        _mqttClient = new MqttFactory().CreateManagedMqttClient();

        ManagedMqttClientOptions mqttOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(mqttBrokerHostname, mqttBrokerPort)
                .WithClientId(ServiceName)
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

        MqttTopicFilter[] mqttSubscriptionTopics = new[] {
            new MqttTopicFilterBuilder()
                .WithTopic(_subscriptionTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build()
        };

        await _mqttClient.SubscribeAsync(mqttSubscriptionTopics);
        await _mqttClient.StartAsync(mqttOptions);

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Stopping {ServiceName} ...");

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task OnMqttMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        string Msg = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        _logger.LogInformation(Msg);

//        _dbCtx = await _dbFactory.CreateDbContextAsync();
        await _dbLogger.WriteAsync(Msg);

        await SendToMqtt( $"{_subscriptionTopic}-Response", Msg);
    }

    public async Task SendToMqtt(string topic, string message)
    {
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(message))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
            .Build();

        await _mqttClient!.EnqueueAsync(mqttMessage);
    }
}