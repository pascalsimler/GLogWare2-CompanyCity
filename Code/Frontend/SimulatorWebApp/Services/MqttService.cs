using Gudel.GLogWare.Messages;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;

namespace SimulatorWebApp.Services;

public class MqttService
{
    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    #endregion

    public MqttService(
        ILogger<MqttService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private IMqttClient? _client;

    public async Task ConnectAsync()
    {
        if (_client != null && _client.IsConnected)
            return;

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        string mqttBrokerIp = "localhost";
        mqttBrokerIp = _configuration[$"MQTTBroker:Ip"] ?? mqttBrokerIp;
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttBrokerIp, 1883)
            .Build();

        await _client.ConnectAsync(options);
    }

    public async Task PublishAsync(string topic, string payload)
    {
        _logger.LogInformation($"topic=[{topic}], payload=[{payload}]");

        if (_client == null || !_client.IsConnected)
            await ConnectAsync();

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(payload))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
            .Build();

        await _client!.PublishAsync(message);
    }

    public async Task SendMessage(string topic, GLogWareMessage m)
    {
        string payload = string.Empty;

        try
        {
            topic = $"{_configuration[$"MQTTBroker:RootTopic"]}/{topic}";
            m.Sender = "Simulator";
            payload = m.Serialize();

            _logger.LogInformation($"topic=[{topic}]");
            _logger.LogInformation($"payload=[\r\n{payload}\r\n]");

            await PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
        }

    }
}