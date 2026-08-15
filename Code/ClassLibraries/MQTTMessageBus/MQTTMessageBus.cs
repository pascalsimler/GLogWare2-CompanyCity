using Gudel.GLogWare.Interfaces;
using Gudel.GLogWare.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

namespace Gudel.GLogWare.MQTTMessageBus;

public class MQTTMessageBus(
    ILogger<MQTTMessageBus> logger,
    IConfiguration configuration
) : IMessageBus
{
    #region Private members
    private string _ip  = "127.0.0.1";
    private int _port = 1883;
    private string _rootTopic = string.Empty;
    private string _clientId = string.Empty;
    private string[] _subscriptionTopics = [];
    private IManagedMqttClient? _mqttClient = null;
    #endregion

    #region Event handlers
    public event EventHandler<MessageBusNotificationEventArgs>? MessageBusNotification;
    #endregion

    #region Public members
    public void Init(string clientId, string[] subscriptionTopics)
    {
        logger.EnterMethod();
        logger.LogKeyValue("clientId", clientId);
        logger.LogKeyValue("subscriptionTopics", string.Join(", ", subscriptionTopics));
        _clientId = clientId;
        _subscriptionTopics = subscriptionTopics;

        string path = "MQTTBroker";
        _ip = configuration[$"{path}:Ip"] ?? _ip;
        if (int.TryParse(configuration[$"{path}:Port"], out int tmpPort)) _port = tmpPort;
        _rootTopic = configuration[$"{path}:RootTopic"] ?? _rootTopic;
       
        logger.LogKeyValue("_ip", _ip);
        logger.LogKeyValue("_port", _port);
        logger.LogKeyValue("_rootTopic", _rootTopic);

        logger.LeaveMethod();
    }

    public async Task StartAsync()
    {
        logger.EnterMethod();

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

        logger.LeaveMethod();
    }

    public async Task PublishAsync(string topic, string message)
    {
        logger.EnterMethod();
        logger.LogKeyValue("topic", topic);
        logger.LogKeyValue("message", message);

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

        logger.LeaveMethod();
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