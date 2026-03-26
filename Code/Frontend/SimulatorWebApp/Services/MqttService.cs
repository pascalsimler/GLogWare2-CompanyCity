using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace SimulatorWebApp.Services;

public class MqttService
{
    private IMqttClient? _client;

    public async Task ConnectAsync()
    {
        if (_client != null && _client.IsConnected)
            return;

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("broker.hivemq.com", 1883) // change to your broker
            .Build();

        await _client.ConnectAsync(options);
    }

    public async Task PublishAsync(string topic, string payload)
    {
        if (_client == null || !_client.IsConnected)
            await ConnectAsync();

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(payload))
            .Build();

        await _client!.PublishAsync(message);
    }
}