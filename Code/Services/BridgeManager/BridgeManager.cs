using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Gudel.GLogWare.BridgeManager;

public class BridgeManager : IHostedService, IAsyncDisposable
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
    // PLC
    private string _plcIp { get; set; } = "127.0.0.1";
    private int _plcPort { get; set; } = 7000;
    private int _plcDelayConnection { get; set; } = 5000;
    private int _plcDelayRetry { get; set; } = 5000;
    private TcpClient? _tcpClient = null;

    //MQTT
    private string _mqttBrokerIp { get; set; } = "127.0.0.1";
    private int _mqttBrokerPort { get; set; } = 1883;
    private string _mqttBrokerRootTopic { get; set; } = string.Empty;
    private IManagedMqttClient? _mqttClient = null;

    // Miscellaneous
    private CancellationTokenSource? _cts;
    #endregion

    public BridgeManager(
        ILogger<BridgeManager> logger,
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
        _ = TcpConnectLoopAsync(_cts.Token);

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
        _logger.LogInformation($"Enter ...");

        // MQTT Broker configuration
        string mqttBrokerConfigPath = "MQTTBroker";
        _mqttBrokerIp = _configuration[$"{mqttBrokerConfigPath}:Ip"] ?? _mqttBrokerIp;
        if (int.TryParse(_configuration[$"{mqttBrokerConfigPath}:Port"], out int tmpMqttBrokerPort)) _mqttBrokerPort = tmpMqttBrokerPort;
        _mqttBrokerRootTopic = _configuration[$"{mqttBrokerConfigPath}:RootTopic"] ?? _mqttBrokerRootTopic;
        _logger.LogInformation($"_mqttBrokerIp=[{_mqttBrokerIp}]");
        _logger.LogInformation($"_mqttBrokerPort=[{_mqttBrokerPort}]");
        _logger.LogInformation($"_mqttBrokerRootTopic=[{_mqttBrokerRootTopic}]");

        // Gantry bridge configuration
        string gantryBridgeConfigPath = $"GantryBridges:{OP}";
        _plcIp = _configuration[$"{gantryBridgeConfigPath}:Ip"] ?? _plcIp;
        if (int.TryParse(_configuration[$"{gantryBridgeConfigPath}:Port"], out int tmpPlcPort)) _plcPort = tmpPlcPort;
        if (int.TryParse(_configuration[$"{gantryBridgeConfigPath}:DelayConnection"], out int tmpPlcDelayConnection)) _plcDelayConnection = tmpPlcDelayConnection;
        if (int.TryParse(_configuration[$"{gantryBridgeConfigPath}:DelayRetry"], out int tmpPlcDelayRetry)) _plcDelayRetry = tmpPlcDelayRetry;
        _logger.LogInformation($"_plcIp=[{_plcIp}]");
        _logger.LogInformation($"_plcPort=[{_plcPort}]");
        _logger.LogInformation($"_plcDelayConnectionPlc=[{_plcDelayConnection}]");
        _logger.LogInformation($"_plcDelayRetry=[{_plcDelayRetry}]");

        _logger.LogInformation($"Leave ...");
    }

    private async Task StartMqtt()
    {
        _mqttClient = new MqttFactory().CreateManagedMqttClient();

        var mqttOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBrokerIp, _mqttBrokerPort)
                .WithClientId($"BridgeManager-{OP}")
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

        string subscriptionTopic = $"{_mqttBrokerRootTopic}/GantryBridges/{OP}/Manager/Incoming";
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

    private async Task TcpConnectLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                _tcpClient = new TcpClient();

                _logger.LogInformation($"Connecting to {_plcIp}:{_plcPort} ...");
                await _tcpClient.ConnectAsync(_plcIp, _plcPort, token);
                _logger.LogInformation($"Connected !");

                using NetworkStream stream = _tcpClient.GetStream();
                await TcpReceiveLoopAsync(stream, token);

                _logger.LogWarning($"Connection closed by the PLC !");
            }
            catch (OperationCanceledException)
            {
                break; // normal termination
            }
            catch (SocketException ex)
            {
                _logger.LogWarning(ex, $"Socket error (Network or PLC inaccessible) !");
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, $"Connection interrupted !");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error !");
            }

            if (!token.IsCancellationRequested)
            {
                _logger.LogInformation($"Reconnecting in {_plcDelayConnection} milliseconds ...");
                await Task.Delay(TimeSpan.FromMilliseconds(_plcDelayConnection), token);
            }

            if (_tcpClient != null)
            {
                _tcpClient.Dispose();
            }
        }

        await Task.CompletedTask;
    }

    private async Task TcpReceiveLoopAsync(NetworkStream stream, CancellationToken token)
    {
        int bytesRead = 0;
        int offset = 0;
        Telegram t;

        try
        {
            while (true)
            {
                offset = 0;
                t = new Telegram();
                while (offset < t.Bytes.Length)
                {
                    bytesRead = await stream.ReadAsync(t.Bytes, offset, t.Bytes.Length - offset, token);
                    if (bytesRead == 0) break; // connection closed properly
                    offset += bytesRead;
                }
                if (bytesRead == 0) break;

                bool isOk = Validate(t);
                _logger.LogInformation($"t.AsciiString=[{t.AsciiString}]");
            }
        }
        catch (OperationCanceledException)
        {
            // normal stop
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Connection interrupted (IO)");
        }
        catch (SocketException ex)
        {
            _logger.LogWarning(ex, "Socket error");
        }

        await Task.CompletedTask;
    }

    public bool Validate(Telegram t)
    {
        byte b;

        t.Parse();
        _logger.LogInformation($"AsciiString=[{t.AsciiString}]");
        _logger.LogInformation($"AckFlag=[{t.AckFlag}]");
        _logger.LogInformation($"Counter=[{t.Counter}]");
        _logger.LogInformation($"Receiver=[{t.Receiver}]");
        _logger.LogInformation($"Sender=[{t.Sender}]");
        _logger.LogInformation($"Name=[{t.Name}]");
        _logger.LogInformation($"Data=[{t.Data}]");
        _logger.LogInformation($"HexaDump=[{t.HexaDump()}]");

        b = t.Bytes[0];
        if (b != DriverConstants.STX)
        {
            _logger.LogError($"Telegramm has wrong start byte: STX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]");
            return false;
        }

        b = t.Bytes[^1];
        if (b != DriverConstants.ETX)
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

        if (t.Receiver != DriverConstants.GLOGWARE_IDENTIFIER)
        {
            _logger.LogError($"Telegram has an invalid Receiver: [{t.Receiver}] != [{DriverConstants.GLOGWARE_IDENTIFIER}]");
            return false;
        }

        //if (!Regex.IsMatch(t.Name, DriverConstants.VALID_RECEIVING_NAMES))
        //{
        //    _logger.LogError($"Telegram has an invalid Name=[{t.Name}] != [{DriverConstants.VALID_RECEIVING_NAMES}]");
        //    return false;
        //}

        return true;
    }

}
