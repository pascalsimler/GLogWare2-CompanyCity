using Gudel.GLogWare.Shared;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace Gudel.GLogWare.BridgeSimulator;

public partial class BridgeSimulator : IHostedService, IAsyncDisposable
{
    #region Public members
    public static string? OP = string.Empty;
    public static string ServiceName = string.Empty;
    #endregion

    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    #endregion

    #region Private members
    //MQTT
    private string _mqttBrokerIp { get; set; } = "127.0.0.1";
    private int _mqttBrokerPort { get; set; } = 1883;
    private string _mqttBrokerRootTopic { get; set; } = string.Empty;
    private IManagedMqttClient? _mqttClient = null;

    // Miscellaneous
    private CancellationTokenSource? _cts;
    #endregion

    public BridgeSimulator(
        ILogger<BridgeSimulator> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LoadConfiguration();

        await StartMqtt();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = TcpAcceptLoopAsync(_cts.Token);

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    private void LoadConfiguration()
    {
        LoadConfigurationMqtt();
        LoadConfigurationGLogWare();
    }

    private void LoadConfigurationMqtt()
    {
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
        _mqttClient = new MqttFactory().CreateManagedMqttClient();

        var mqttOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBrokerIp, _mqttBrokerPort)
                .WithClientId($"BridgeSimulator-{OP}")
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

        string subscriptionTopic = $"{_mqttBrokerRootTopic}/GantryBridges/{OP}/Simulation/Incoming";
        _logger.LogInformation($"subscriptionTopic=[{subscriptionTopic}]");

        var mqttSubscriptionTopics = new[] {
            new MqttTopicFilterBuilder()
                .WithTopic(subscriptionTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build()
        };

        await _mqttClient.SubscribeAsync(mqttSubscriptionTopics);
        await _mqttClient.StartAsync(mqttOptions);
    }

    public async Task OnMqttMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        //_plcSendingReleased.WaitOne();
        //_plcSendingReleased.Reset();

        string Msg = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        _logger.LogInformation(Msg);

        //PlcTelegram t = new PlcTelegram();
        //t.Name = "ORDS";
        //t.Sender = DriverConstant.GLOGWARE_IDENTIFIER;
        //t.Receiver = _OP;
        //t.Data = Msg;
        //_plcCommunication.Send(t);
    }

    private async void OnWatchdogRetry(object source, ElapsedEventArgs e)
    {
        _watchdogRetry.Enabled = false;
        await SendToPlc(_lastSentTelegram, false);
        _watchdogRetry.Enabled = true;
    }

    public async Task SendToPlc(GLogWareTelegram t, bool isNew = false)
    {
        try
        {
            if (isNew)
            {
                t.AckFlag = "1";
                if (_lastSentTelegram.Counter == string.Empty)
                {
                    t.Counter = "0";
                }
                else
                {
                    int counter = int.Parse(_lastSentTelegram.Counter);
                    counter++;
                    if (counter > 9) counter = 1;
                    t.Counter = $"{counter:0}";
                }
            }

            t.Build();

            if (_tcpClient != null)
            {
                if (_tcpClient.Connected)
                {
                    _logger.LogInformation(t.AsciiString);
                    NetworkStream stream = _tcpClient.GetStream();
                    await stream.WriteAsync(t.Bytes, 0, t.Bytes.Length);
                    if (isNew)
                    {
                        _lastSentTelegram = t;
                        _watchdogRetry!.Enabled = true;
                    }
                }
                else
                {
                    _logger.LogError($"_tcpClient is not connected !");
                }
            }
            else
            {
                _logger.LogError($"_tcpClient is null !");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error !");
        }
    }

    private bool Validate(GLogWareTelegram t)
    {
        byte b;

        t.Parse();
        //_logger.LogInformation($"AsciiString=[{t.AsciiString}]");
        //_logger.LogInformation($"AckFlag=[{t.AckFlag}]");
        //_logger.LogInformation($"Counter=[{t.Counter}]");
        //_logger.LogInformation($"Receiver=[{t.Receiver}]");
        //_logger.LogInformation($"Sender=[{t.Sender}]");
        //_logger.LogInformation($"Identifier=[{t.Identifier}]");
        //_logger.LogInformation($"Data=[{t.Data}]");
        //_logger.LogInformation($"HexaDump=[{t.HexaDump()}]");

        b = t.Bytes[0];
        if (b != GLogWareTelegramConstants.STX)
        {
            _logger.LogError($"Telegramm has wrong start byte: STX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]");
            return false;
        }

        b = t.Bytes[^1];
        if (b != GLogWareTelegramConstants.ETX)
        {
            _logger.LogError($"Telegramm has wrong end byte: ETX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]");
            return false;
        }

        if (!Regex.IsMatch(t.AckFlag, @"^[0-1]$"))
        {
            _logger.LogError($"Telegram has invalid AckFlag=[{t.AckFlag}]. Expected values are: [0]=Acknowledge not required, [1]=Acknowledge required");
            return false;
        }

        if (!Regex.IsMatch(t.Counter, @"^[0-9]$"))
        {
            _logger.LogError($"Telegram has invalid Counter=[{t.Counter}]");
            return false;
        }

        if (t.Receiver != OP)
        {
            _logger.LogError($"Telegram has an invalid Receiver. (Is=[{t.Receiver}]) != (Should=[{OP}]");
            return false;
        }

        if (t.Sender != GLogWareTelegramConstants.GLOGWARE_IDENTIFIER)
        {
            _logger.LogError($"Telegram has an invalid Sender. (Is=[{t.Sender}]) != (Should=[{GLogWareTelegramConstants.GLOGWARE_IDENTIFIER}])");
            return false;
        }

        //if (!Enum.TryParse<TelegramReceiveIdentifiers>(t.Identifier, out _))
        //{
        //    string validValues = string.Join("|", Enum.GetNames<TelegramReceiveIdentifiers>());
        //    _logger.LogError($"Telegram has an invalid Identifier. (Is=[{t.Identifier}]) != (Should=[{validValues}])");
        //    return false;
        //}

        return true;
    }
}