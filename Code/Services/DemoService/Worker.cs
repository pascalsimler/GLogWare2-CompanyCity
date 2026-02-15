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
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<GLogWareDbContext> _factory;
    private GLogWareDbContext _db = null!;
    private DbLogger _dbLogger = null!;
    private IManagedMqttClient _mqttClient = null!;
    private System.Timers.Timer _timer = null!;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private string _subscriptionTopic = null!;
    #endregion

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> factory)
    {
        _logger = logger;
        _configuration = configuration;
        _factory = factory;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting {ServiceName} ...");

        //#region Connect to database
        //_db = await _factory.CreateDbContextAsync(stoppingToken);
        //_dbLogger = new DbLogger(_db)!;
        //#endregion

        #region Timer
        _timer = new System.Timers.Timer(2000);
        _timer.Elapsed += OnTimer;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        #endregion

        #region Connect to MQTT
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
        #endregion

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
        await _mutex.WaitAsync();
        _timer.Enabled = false;
        try
        {
            string Msg = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            _logger.LogInformation(Msg);
            _db = await _factory.CreateDbContextAsync();
            _dbLogger = new DbLogger(_db)!;
            await _dbLogger.WriteAsync(Msg);
            await SendToMqtt($"{_subscriptionTopic}-Response", Msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
        _timer.Enabled = true;
        _mutex.Release();
    }

    private async void OnTimer(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await _mutex.WaitAsync();
        _timer.Enabled = false;
        try
        {
            _logger.LogInformation($"Timer fired at {DateTime.Now}");
            System.Threading.Thread.Sleep(10000);
            _logger.LogInformation($"Timer quit at {DateTime.Now}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
        _timer.Enabled = true;
        _mutex.Release();
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