using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Text;

namespace Gudel.GLogWare.DemoService;

public class DemoService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<GLogWareDbContext> _factory;
    private readonly DbLogger _dbLogger;
    private IManagedMqttClient _mqttClient = null!;

    public DemoService(
        ILogger<DemoService> logger,
        DbLogger dbLogger,
        IDbContextFactory<GLogWareDbContext> factory)
    {
        _logger = logger;
        _dbLogger = dbLogger;
        _factory = factory;
    }

    public void SetMqttClient(IManagedMqttClient mqttClient)
    {
        _mqttClient = mqttClient;
    }

    public async Task HandleMqttMessageAsync(string topic, string message)
    {
        try
        {
            string logMsg = $"topic=[{topic}], message=[{message}]";
            _logger.LogInformation(logMsg);
            await using var db = await _factory.CreateDbContextAsync();
            await _dbLogger.WriteAsync(logMsg);
            await Task.Delay(5000);
            await SendToMqtt($"{topic}-Response", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    public async Task HandleTimerAsync()
    {
        try
        {
            _logger.LogInformation($"Timer fired at {DateTime.Now}");
            using var db = await _factory.CreateDbContextAsync();
            {
                foreach (var area in db.Areas)
                {
                    string logMsg = $"area=[{area.Name}]";
                    _logger.LogInformation(logMsg);
                    await _dbLogger.WriteAsync(logMsg);
                    await Task.Delay(1000);
                }
            }
            _logger.LogInformation($"Timer quit at {DateTime.Now}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }


    private async Task SendToMqtt(string topic, string message)
    {
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(message))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
            .Build();

        await _mqttClient!.EnqueueAsync(mqttMessage);
    }
}
