using Gudel.GLogWare.Logging;
using Gudel.GLogWare.MessageBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

namespace Gudel.GLogWare.MQTTMessageBus;

public class MQTTMessageBus : IMessageBus
{
    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    #endregion

    #region Private members
    private string _ip { get; set; } = "127.0.0.1";
    private int _port { get; set; } = 1883;
    private string _rootTopic { get; set; } = string.Empty;
    private string _clientId { get; set; } = string.Empty;
    private string[] _subscriptionTopics { get; set; } = Array.Empty<string>();
    private IManagedMqttClient? _mqttClient = null;
    #endregion

    #region Event handlers
    public event EventHandler<MessageBusNotificationEventArgs>? MessageBusNotification;
    #endregion

    #region Constructors
    public MQTTMessageBus(
        ILogger<MQTTMessageBus> logger,
        IConfiguration configuration
    ) 
    {
        _logger = logger;
        _configuration = configuration;
    }
    #endregion

    #region Public members
    public void Init(string clientId, string[] subscriptionTopics)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"clientId=[{clientId}]");
        _logger.LogInformation($"subscriptionTopics=[{string.Join(", ", subscriptionTopics)}]");
        _clientId = clientId;
        _subscriptionTopics = subscriptionTopics;

        string path = "MQTTBroker";
        _ip = _configuration[$"{path}:Ip"] ?? _ip;
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpPort)) _port = tmpPort;
        _rootTopic = _configuration[$"{path}:RootTopic"] ?? _rootTopic;
       
        _logger.LogInformation($"_ip=[{_ip}]");
        _logger.LogInformation($"_port=[{_port}]");
        _logger.LogInformation($"_rootTopic=[{_rootTopic}]");

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    public async Task StartAsync()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _mqttClient = new MqttFactory().CreateManagedMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
        _mqttClient.ConnectedAsync += OnConnected;
        _mqttClient.DisconnectedAsync += OnDisconnected;

        var mqttOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_ip, _port)
                .WithClientId(_clientId)
                .WithCleanSession(false)
                .Build())
            .Build();

        var filters = _subscriptionTopics
            .Select(topic =>
                new MqttTopicFilterBuilder()
                    .WithTopic($"{_rootTopic}/{topic}")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build())
            .ToList();
        await _mqttClient.SubscribeAsync(filters);

        await _mqttClient.StartAsync(mqttOptions);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    public async Task PublishAsync(string topic, string message)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"topic=[{topic}]");
        _logger.LogInformation($"message=[{message}]");

        var mqttMessage =
            new MqttApplicationMessageBuilder()
                .WithTopic($"{_rootTopic}/{topic}")
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

        if (_mqttClient != null)
        {
            await _mqttClient.EnqueueAsync(mqttMessage);
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }
    #endregion region

    #region Private members
    private Task OnConnected(MqttClientConnectedEventArgs args)
    {
        RaiseNotification(
            MessageBusNotificationType.Connected);

        return Task.CompletedTask;
    }

    private Task OnDisconnected(MqttClientDisconnectedEventArgs args)
    {
        RaiseNotification(
            MessageBusNotificationType.Disconnected);

        return Task.CompletedTask;
    }

    private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        RaiseNotification(
            MessageBusNotificationType.MessageReceived,
            args.ApplicationMessage.Topic,
            args.ApplicationMessage.ConvertPayloadToString());

        return Task.CompletedTask;
    }

    private void RaiseNotification(
        MessageBusNotificationType type,
        string topic = "",
        string payload = "")
    {
        MessageBusNotification?.Invoke(
            this,
            new MessageBusNotificationEventArgs
            {
                NotificationType = type,
                Topic = topic,
                Payload = payload
            });
    }
    #endregion
}