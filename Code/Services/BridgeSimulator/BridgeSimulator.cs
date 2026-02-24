using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Gudel.GLogWare.BridgeSimulator;

public class BridgeSimulator : IHostedService, IAsyncDisposable
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
    private int _plcPort { get; set; } = 7000;
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
        if (int.TryParse(_configuration[$"{gantryBridgeConfigPath}:Port"], out int tmpPlcPort)) _plcPort = tmpPlcPort;
        if (int.TryParse(_configuration[$"{gantryBridgeConfigPath}:DelayRetry"], out int tmpPlcDelayRetry)) _plcDelayRetry = tmpPlcDelayRetry;
        _logger.LogInformation($"_plcPort=[{_plcPort}]");
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

    private async Task TcpAcceptLoopAsync(CancellationToken token)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, _plcPort);
        listener.Start();
        _logger.LogInformation($"Listening on port {_plcPort} ...");

        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Waiting for a new incoming connection request!");
                    _tcpClient = await listener.AcceptTcpClientAsync(token);
                    _logger.LogInformation($"Client connected from {_tcpClient.Client.RemoteEndPoint} !");

                    using NetworkStream stream = _tcpClient.GetStream();
                    await TcpReceiveLoopAsync(stream, token);

                    _logger.LogWarning($"Connection closed by the client !");
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }
                catch (OperationCanceledException)
                {
                    break; // normal termination
                }
                catch (SocketException ex)
                {
                    _logger.LogWarning(ex, $"Socket error !");
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, $"Connection interrupted !");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error !");
                }
            }
        }
        finally
        {
            listener.Stop();
            _logger.LogInformation($"Listener stopped.");
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

                //bool isOk = Validate(plcTelegram);
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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}