using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Text;

namespace Gudel.GLogWare.Services.DemoService;

public class DemoService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<GLogWareDbContext> _dbContextFactory;
    private IManagedMqttClient _mqttClient = null!;

    public DemoService(
        ILogger<DemoService> logger,
        IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
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
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            Protocol protocol = new Protocol();
            protocol.Message = logMsg;
            //await _dbLoggerService.WriteProtocolAsync(protocol);
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
            using var db = await _dbContextFactory.CreateDbContextAsync();
            {
                foreach (var area in db.Areas)
                {
                    string logMsg = $"area=[{area.Name}]";
                    _logger.LogInformation(logMsg);
                    Protocol protocol = new Protocol();
                    protocol.Message = logMsg;
                    //await _dbLoggerService.WriteProtocolAsync(protocol);
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
