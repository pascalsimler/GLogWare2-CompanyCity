using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System.Text;
using System.Timers;

namespace Gudel.GLogWare.Services.Reserve;

public partial class Reserve: IHostedService, IAsyncDisposable
{
    #region Public member
    public static string ServiceName { get; set; } = string.Empty;
    #endregion

    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
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
    private System.Timers.Timer _watchdogWakeup = null!;
    private int _delayWakeup { get; set; } = 30000;
    private SemaphoreSlim _semaphoreLock = null!;
    #endregion

    #region Constructor
    public Reserve(
        ILogger<Reserve> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
    }
    #endregion

    private void LoadConfiguration()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        string path = "MQTTBroker";
        _mqttBrokerIp = _configuration[$"{path}:Ip"] ?? _mqttBrokerIp;
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpMqttBrokerPort)) _mqttBrokerPort = tmpMqttBrokerPort;
        _mqttBrokerRootTopic = _configuration[$"{path}:RootTopic"] ?? _mqttBrokerRootTopic;
        if (int.TryParse(_configuration[$"{path}:DelayWakeup"], out int tmpDelayWakeup)) _delayWakeup = tmpDelayWakeup;

        _logger.LogInformation($"_mqttBrokerIp=[{_mqttBrokerIp}]");
        _logger.LogInformation($"_mqttBrokerPort=[{_mqttBrokerPort}]");
        _logger.LogInformation($"_mqttBrokerRootTopic=[{_mqttBrokerRootTopic}]");
        _logger.LogInformation($"_delayWakeup=[{_delayWakeup}]");

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task StartMqtt()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _mqttClient = new MqttFactory().CreateManagedMqttClient();

        ManagedMqttClientOptions mqttOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBrokerIp, _mqttBrokerPort)
                .WithClientId(ServiceName)
                .WithCleanSession(false)
                .Build())
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e => {
            await OnMqttMessageReceived(e);
            await Task.CompletedTask;
        };

        _mqttClient.ConnectedAsync += async e => {
            _logger.LogInformation($"Connected to MQTT broker.");
            await Task.CompletedTask;
        };

        _mqttClient.DisconnectedAsync += async e => {
            _logger.LogInformation($"Disconnected from MQTT broker.");
            await Task.CompletedTask;
        };

        _subscriptionTopic = $"{_mqttBrokerRootTopic}/{ServiceName}/Incoming";
        _logger.LogInformation($"subscriptionTopic=[{_subscriptionTopic}]");

        MqttTopicFilter[] mqttSubscriptionTopics = new[] {
            new MqttTopicFilterBuilder()
                .WithTopic(_subscriptionTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build()
        };

        await _mqttClient.SubscribeAsync(mqttSubscriptionTopics);
        await _mqttClient.StartAsync(mqttOptions);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task OnMqttMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await Lock();
        try
        {
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            _logger.LogInformation(payload);

            GLogWareMessage m = GLogWareMessage.DeSerialize(payload)!;
            switch (m.Identifier)
            {
                case GLogWareMessageIdentifiers.WakeUp:
                    //await DoWork();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GLogWareMessage");
        }
        finally
        {
            Unlock();
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task SendGLogWareMessageToMqtt(string topic, GLogWareMessage m)
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

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        LoadConfiguration();

        _semaphoreLock = new SemaphoreSlim(1);

        await StartMqtt();

        _watchdogWakeup = new System.Timers.Timer(_delayWakeup);
        _watchdogWakeup.Elapsed += OnWatchdogWakeup!;
        _watchdogWakeup.AutoReset = true;
        _watchdogWakeup.Start();

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    private async void OnWatchdogWakeup(object source, ElapsedEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await SendWakeUp(_subscriptionTopic, "Wakeup timer ellapsed");

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task SendWakeUp(string topic, string message)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"topic=[{topic}]");
        _logger.LogInformation($"message=[{message}]");

        GLogWareMessage gm = new GLogWareMessage();
        gm.Identifier = GLogWareMessageIdentifiers.WakeUp;
        gm.Data = message;
        await SendGLogWareMessageToMqtt(_subscriptionTopic, gm);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task Lock()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _watchdogWakeup.Stop();
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
        _watchdogWakeup.Start();

        _logger.LogInformation(LogMessages.LeaveMethod);
    }
}
