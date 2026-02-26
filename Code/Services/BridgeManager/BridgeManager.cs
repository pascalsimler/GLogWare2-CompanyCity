using Gudel.GLogWare.EFCore.Application;
using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

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
    private readonly GLogWareDbContext _db;
    private readonly DbLoggerService _dbLoggerService;
    #endregion

    #region Private members

    // PLC
    private string _plcIp { get; set; } = "127.0.0.1";
    private int _plcPort { get; set; } = 7000;
    private int _plcDelayConnection { get; set; } = 5000;
    private int _plcDelayRetry { get; set; } = 5000;
    private TcpClient? _tcpClient = null;
    private string _lastReceivedCounter = "0";
    private Telegram _lastSentTelegram = null!;
    private Telegram _ackTelegram = null!;
    private System.Timers.Timer _watchdogRetry = null!;

    // MQTT
    private string _mqttBrokerIp { get; set; } = "127.0.0.1";
    private int _mqttBrokerPort { get; set; } = 1883;
    private string _mqttBrokerRootTopic { get; set; } = string.Empty;
    private IManagedMqttClient? _mqttClient = null;

    // Miscellaneous
    private CancellationTokenSource? _cts;
    private LogPlc _lpReceive = null!;
    private LogPlc _lpSend = null!;

    #endregion

    public BridgeManager(
        ILogger<BridgeManager> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> factory,
        DbLoggerService dbLoggerService)
    {
        _logger = logger;
        _configuration = configuration;
        _db = factory.CreateDbContext();
        _dbLoggerService = dbLoggerService;
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
        string path = string.Empty; 
        
        // MQTT Broker configuration
        path = "MQTTBroker";
        _mqttBrokerIp = _configuration[$"{path}:Ip"] ?? _mqttBrokerIp;
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpMqttBrokerPort)) _mqttBrokerPort = tmpMqttBrokerPort;
        _mqttBrokerRootTopic = _configuration[$"{path}:RootTopic"] ?? _mqttBrokerRootTopic;
        _logger.LogInformation($"_mqttBrokerIp=[{_mqttBrokerIp}]");
        _logger.LogInformation($"_mqttBrokerPort=[{_mqttBrokerPort}]");
        _logger.LogInformation($"_mqttBrokerRootTopic=[{_mqttBrokerRootTopic}]");

        // Gantry bridge configuration
        path = $"GantryBridges:{OP}";
        _plcIp = _configuration[$"{path}:Ip"] ?? _plcIp;
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpPlcPort)) _plcPort = tmpPlcPort;
        if (int.TryParse(_configuration[$"{path}:DelayConnection"], out int tmpPlcDelayConnection)) _plcDelayConnection = tmpPlcDelayConnection;
        if (int.TryParse(_configuration[$"{path}:DelayRetry"], out int tmpPlcDelayRetry)) _plcDelayRetry = tmpPlcDelayRetry;
        _logger.LogInformation($"_plcIp=[{_plcIp}]");
        _logger.LogInformation($"_plcPort=[{_plcPort}]");
        _logger.LogInformation($"_plcDelayConnectionPlc=[{_plcDelayConnection}]");
        _logger.LogInformation($"_plcDelayRetry=[{_plcDelayRetry}]");
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

        Telegram t = new Telegram();
        t.Identifier = TelegramSendIdentifiers.LIFE.ToString();
        t.Sender = TelegramConstants.GLOGWARE_IDENTIFIER;
        t.Receiver = OP!;
        t.Data = string.Empty;
        await SendToPlc(t, true);
    }

    private async Task TcpConnectLoopAsync(CancellationToken token)
    {
        string information = string.Empty;

        _lastSentTelegram = new Telegram();
        _ackTelegram = new Telegram();
        _watchdogRetry = new System.Timers.Timer(_plcDelayRetry);
        _watchdogRetry.Elapsed += OnWatchdogRetry!;
        _watchdogRetry.AutoReset = true;
        _watchdogRetry.Enabled = false;

        while (!token.IsCancellationRequested)
        {
            try
            {
                _tcpClient = new TcpClient();

                _logger.LogInformation($"Connecting to {_plcIp}:{_plcPort} ...");
                await _tcpClient.ConnectAsync(_plcIp, _plcPort, token);
                information = $"Connected to {_plcIp}:{_plcPort} !";
                _logger.LogInformation(information);
                {
                    LogPlc lp = new LogPlc();
                    InitLogPlc(lp);
                    lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
                    lp.Information = information;
                    await _dbLoggerService.WriteLogPlcAsync(lp);
                }

                using NetworkStream stream = _tcpClient.GetStream();
                await TcpReceiveLoopAsync(stream, token);

                information = $"Connection closed by the PLC !";
                _logger.LogWarning(information);
                {
                    LogPlc lp = new LogPlc();
                    InitLogPlc(lp);
                    lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
                    lp.Information = information;
                    await _dbLoggerService.WriteLogPlcAsync(lp);
                }
            }
            catch (OperationCanceledException ex)
            {
                information = $"Normal termination";
                _logger.LogWarning(ex, information);
                LogPlc lp = new LogPlc();
                InitLogPlc(lp);
                lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
                lp.Information = information;
                lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
                await _dbLoggerService.WriteLogPlcAsync(lp);
                break;
            }
            catch (SocketException ex)
            {
                information = $"Socket error (Network or PLC inaccessible) !";
                _logger.LogWarning(ex, information);
                LogPlc lp = new LogPlc();
                InitLogPlc(lp);
                lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
                lp.Information = information;
                lp.Data = $"{ex.Source} ({ex.NativeErrorCode}): {ex.Message}\r\n{ex.StackTrace}";
                await _dbLoggerService.WriteLogPlcAsync(lp);
            }
            catch (IOException ex)
            {
                information = $"Connection interrupted !";
                _logger.LogWarning(ex, information);
                LogPlc lp = new LogPlc();
                InitLogPlc(lp);
                lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
                lp.Information = information;
                lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
                await _dbLoggerService.WriteLogPlcAsync(lp);
            }
            catch (Exception ex)
            {
                information = $"Unexpected error !";
                _logger.LogError(ex, information);
                LogPlc lp = new LogPlc();
                InitLogPlc(lp);
                lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
                lp.Information = information;
                lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
                await _dbLoggerService.WriteLogPlcAsync(lp);
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
        string information = string.Empty;
        Telegram t = new Telegram();

        try
        {
            while (true)
            {
                offset = 0;
                Array.Clear(t.Bytes, 0, t.Bytes.Length);
                while (offset < t.Bytes.Length)
                {
                    bytesRead = await stream.ReadAsync(t.Bytes, offset, t.Bytes.Length - offset, token);
                    if (bytesRead == 0) break; // connection closed properly
                    offset += bytesRead;
                }
                if (bytesRead == 0)
                {
                    information = $"Connection closed by the PLC !";
                    _logger.LogWarning(information);
                    break;
                }

                await ProcessTelegram(t);
            }
        }
        catch (OperationCanceledException ex)
        {
            information = $"Normal termination";
            _logger.LogWarning(ex, information);
            LogPlc lp = new LogPlc();
            InitLogPlc(lp);
            lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
            lp.Information = information;
            lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
            await _dbLoggerService.WriteLogPlcAsync(lp);
        }
        catch (IOException ex)
        {
            information = $"Connection interrupted !";
            _logger.LogWarning(ex, information);
            LogPlc lp = new LogPlc();
            InitLogPlc(lp);
            lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
            lp.Information = information;
            lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
            await _dbLoggerService.WriteLogPlcAsync(lp);
        }
        catch (Exception ex)
        {
            information = $"Unexpected error !";
            _logger.LogError(ex, information);
            LogPlc lp = new LogPlc();
            InitLogPlc(lp);
            lp.Direction = LogPlcDirectionNames.GENERAL.ToString();
            lp.Information = information;
            lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
            await _dbLoggerService.WriteLogPlcAsync(lp);
        }

        await Task.CompletedTask;
    }

    private async Task ProcessTelegram(Telegram t)
    {
        _lpReceive = new LogPlc();
        InitLogPlc(_lpReceive);
        _lpReceive.Direction = LogPlcDirectionNames.PLC_TO_GLOGWARE.ToString(); 

        if (!Validate(t))
        {
            _lpReceive.Data = t.HexaDump();
            _logger.LogWarning(_lpReceive.Data);
            await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
            return;
        }

        _logger.LogInformation(t.AsciiString);
        if (t.Identifier == TelegramReceiveIdentifiers.ACKN.ToString())
        {
            if (t.Counter == _lastSentTelegram.Counter)
            {
                _watchdogRetry.Enabled = false;
                //if (sendingReleased != null)
                //    sendingReleased.Invoke(this, new SendingReleasedEventArgs());
            }
            else
            {
                _lpReceive.Information =
                     $"Unexpected counter in ACKN: " +
                     $"Is=[{t.Counter}], ShouldBe=[{_lastSentTelegram.Counter}]";
                _lpReceive.Data = t.HexaDump();
                _logger.LogError(_lpReceive.Information);
                await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
            }
        }
        else
        {
            _ackTelegram.Sender = t.Receiver;
            _ackTelegram.Receiver = t.Sender;
            _ackTelegram.Identifier = TelegramSendIdentifiers.ACKN.ToString();
            _ackTelegram.AckFlag = "0";
            _ackTelegram.Counter = t.Counter;
            _ackTelegram.Data = t.Data;
            await SendToPlc(_ackTelegram, false);
            if (t.Counter == _lastReceivedCounter && t.Counter != "0")
            {
                _lpReceive.Information =
                    $"Same counter [{t.Counter}] as previous telegram. " +
                    $"It is a retry telegram --> No processing";
                _lpReceive.Data = t.HexaDump();
                _logger.LogError(_lpReceive.Information);
                await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
            }
            else
            {
                _lastReceivedCounter = t.Counter;
                //if (telegramReceived != null)
                //    telegramReceived.Invoke(this, new TelegramReceivedEventArgs(_receivedTelegram));
            }
        }
    }

    private async void OnWatchdogRetry(object source, ElapsedEventArgs e)
    {
        _watchdogRetry.Enabled = false;
        await SendToPlc(_lastSentTelegram, false);
        _watchdogRetry.Enabled = true;
    }

    public async Task SendToPlc(Telegram t, bool isNew = false)
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
                    _logger.LogInformation($"{t.AsciiString}");
                    //_logger.LogInformation($"Hexa: {t.HexaDump()}");
                    NetworkStream stream = _tcpClient.GetStream();
                    await stream.WriteAsync(t.Bytes, 0, t.Bytes.Length);
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

        if (isNew)
        {
            _lastSentTelegram = t;
            _watchdogRetry.Enabled = true;
        }
    }

    private bool Validate(Telegram t)
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

        _lpReceive.Ackflag = t.AckFlag;
        _lpSend.Counter = t.Counter;
        _lpReceive.Sender = t.Sender;
        _lpSend.Receiver = t.Receiver;
        _lpSend.Identifier = t.Identifier;

        b = t.Bytes[0];
        if (b != TelegramConstants.STX)
        {
            _lpReceive.Information = 
                $"Telegramm has wrong start byte: " +
                $"STX != [Hexa:0x{b.ToString("X2")} - " +
                $"Decimal:{b} - " +
                $"ASCII:{((char)b).ToString()}]";
            _logger.LogError(_lpReceive.Information);
            return false;
        }

        b = t.Bytes[^1];
        if (b != TelegramConstants.ETX)
        {
            _lpReceive.Information =
                $"Telegramm has wrong end byte: " +
                $"ETX != [Hexa:0x{b.ToString("X2")} - " +
                $"Decimal:{b} - " + 
                $"ASCII:{((char)b).ToString()}]";
            _logger.LogError(_lpReceive.Information);
            return false;
        }

        if (!Regex.IsMatch(t.AckFlag, @"^[0-1]$"))
        {
            _lpReceive.Information =
                $"Telegram has invalid AckFlag=[{t.AckFlag}]. " +
                $"Expected values are: [0]=Acknowledge not required, [1]=Acknowledge required";
            _logger.LogError(_lpReceive.Information);
            return false;
        }

        if (!Regex.IsMatch(t.Counter, @"^[0-9]$"))
        {
            _lpReceive.Information =
                $"Telegram has invalid Counter=[{t.Counter}]";
            _logger.LogError(_lpReceive.Information);
            return false;
        }

        if (t.Receiver != TelegramConstants.GLOGWARE_IDENTIFIER)
        {
            _lpReceive.Information =
                $"Telegram has an invalid Receiver. " +
                $"(Is=[{t.Receiver}]) != (Should=[{TelegramConstants.GLOGWARE_IDENTIFIER}]";
            _logger.LogError(_lpReceive.Information);
            return false;
        }

        if (t.Sender != OP)
        {
            _lpReceive.Information =
                $"Telegram has an invalid Sender. " +
                $"(Is=[{t.Sender}]) != (Should=[{OP}])";
            _logger.LogError(_lpReceive.Information);
            return false;
        }

        if (!Enum.TryParse<TelegramReceiveIdentifiers>(t.Identifier, out _))
        {
            string validValues = string.Join("|", Enum.GetNames<TelegramReceiveIdentifiers>());
            _lpReceive.Information =
                $"Telegram has an invalid Identifier. " +
                $"(Is=[{t.Identifier}]) != (Should=[{validValues}])";
            _logger.LogError(_lpReceive.Information);
            return false;
        }

        return true;
    }

    private void InitLogPlc(LogPlc lp)
    {
        lp.Category = LogPlcCategoryNames.GANTRY.ToString();
        lp.Process = ServiceName;
    }

}