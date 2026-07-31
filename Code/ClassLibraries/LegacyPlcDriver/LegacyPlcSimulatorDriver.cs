using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Gudel.GLogWare.PlcDriver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Gudel.GLogWare.LegacyPlcDriver;

public class LegacyPlcSimulatorDriver : IPlcDriver
{
    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    #endregion

    #region Driver parameters
    private string _op = string.Empty;
    private int _port { get; set; } = 7000;
    private int _delayRetry { get; set; } = 5000;
    private string _validIdentifiers { get; set; } = string.Empty;
    #endregion

    #region Private members
    private TcpClient? _tcpClient = null;
    private string _lastReceivedCounter = "0";
    private LegacyPlcTelegram _lastSentTelegram = null!;
    private PlcMessage _lastSentPlcMessage = null!;
    private LegacyPlcTelegram _ackTelegram = null!;
    private System.Timers.Timer _watchdogRetry = null!;
    private SemaphoreSlim _semaphoreSend = null!;
    private DriverNotificationEventArgs _driverNotificationEventArgs = null!;
    #endregion

    #region Event handlers
    public event EventHandler<DriverNotificationEventArgs>? DriverNotification;
    #endregion

    #region Constructors
    public LegacyPlcSimulatorDriver(
        ILogger<LegacyPlcDriver> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    #endregion

    #region Public methods
    public void LoadConfiguration(string path)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"path=[{path}]");

        _op = path.Substring(path.LastIndexOf(':') + 1);
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpPort)) _port = tmpPort;
        if (int.TryParse(_configuration[$"{path}:DelayRetry"], out int tmpDelayRetry)) _delayRetry = tmpDelayRetry;
        _validIdentifiers = _configuration[$"{path}:ValidGLogWareIdentifiers"] ?? string.Empty;

        _logger.LogInformation($"_op=[{_op}]");
        _logger.LogInformation($"_port=[{_port}]");
        _logger.LogInformation($"_delayRetry=[{_delayRetry}]");
        _logger.LogInformation($"_validIdentifiers=[{_validIdentifiers}]");

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = TcpAcceptLoopAsync(cts.Token);

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    public async Task SendAsync(PlcMessage plcMessage)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        LegacyPlcTelegram t = new LegacyPlcTelegram();
        t.Identifier = plcMessage.Identifier.ToString();
        t.Sender = plcMessage.Sender;
        t.Receiver = plcMessage.Receiver;
        switch (plcMessage.Identifier)
        {
            case PlcMessageIdentifiers.STAT:
                STATBridge stat = GLogWareMessage.DeSerialize<STATBridge>(plcMessage.Data!.ToString()!)!;
                STATBridgeStruct statStruct = STATBridgeStruct.FromMessage(stat);
                t.Data = statStruct.ToData();
                break;
            case PlcMessageIdentifiers.COMP:
                COMP comp = GLogWareMessage.DeSerialize<COMP>(plcMessage.Data!.ToString()!)!;
                COMPStruct compStruct = COMPStruct.FromMessage(comp);
                t.Data = compStruct.ToData();
                break;
            default:
                t.Data = string.Empty;
                break;
        }
        await SendToGLogWareAsync(t, true);
        _lastSentPlcMessage = plcMessage;
        _driverNotificationEventArgs = new DriverNotificationEventArgs();
        _driverNotificationEventArgs.NotificationType = DriverNotificationType.TelegramSent;
        _driverNotificationEventArgs.PlcMessage = _lastSentPlcMessage;
        DriverNotification?.Invoke(this, _driverNotificationEventArgs);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }
    #endregion

    #region Private methods
    private async Task TcpAcceptLoopAsync(CancellationToken token)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _lastSentTelegram = new LegacyPlcTelegram();
        _ackTelegram = new LegacyPlcTelegram();

        _semaphoreSend = new SemaphoreSlim(1);

        _watchdogRetry = new System.Timers.Timer(_delayRetry);
        _watchdogRetry.Elapsed += OnWatchdogRetryAsync!;
        _watchdogRetry.AutoReset = true;
        _watchdogRetry.Stop();

        TcpListener listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();
        _logger.LogInformation($"Listening on port {_port} ...");

        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Waiting for a new incoming connection request!");
                    _tcpClient = await listener.AcceptTcpClientAsync(token);
                    _logger.LogInformation($"Client connected from {_tcpClient.Client.RemoteEndPoint} !");
                    _driverNotificationEventArgs = new DriverNotificationEventArgs();
                    _driverNotificationEventArgs.NotificationType = DriverNotificationType.Online;
                    DriverNotification?.Invoke(this, _driverNotificationEventArgs);

                    //await SendCurrentSTAT();

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
                _driverNotificationEventArgs = new DriverNotificationEventArgs();
                _driverNotificationEventArgs.NotificationType = DriverNotificationType.Offline;
                DriverNotification?.Invoke(this, _driverNotificationEventArgs);
            }
        }
        finally
        {
            listener.Stop();
            _logger.LogInformation($"Listener stopped.");
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    private async Task TcpReceiveLoopAsync(NetworkStream stream, CancellationToken token)
    {
        int bytesRead = 0;
        int offset = 0;
        LegacyPlcTelegram t = new LegacyPlcTelegram();

        _logger.LogInformation(LogMessages.EnterMethod);

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
                if (bytesRead == 0) break;

                await ProcessTelegramAsync(t);
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

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    private async Task ProcessTelegramAsync(LegacyPlcTelegram t)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        string logMsg = string.Empty;

        while (true)
        {
            if (!ValidateTelegram(t))
            {
                _logger.LogWarning(t.HexaDump());
                break;
            }

            _logger.LogInformation(t.AsciiString);

            if (t.Identifier == PlcMessageIdentifiers.ACKN.ToString())
            {
                if (_watchdogRetry.Enabled)
                {
                    if (t.Counter == _lastSentTelegram.Counter)
                    {
                        _watchdogRetry.Stop();
                        _semaphoreSend.Release();
                        _driverNotificationEventArgs = new DriverNotificationEventArgs();
                        _driverNotificationEventArgs.NotificationType = DriverNotificationType.TelegramSentAcknowledged;
                        _driverNotificationEventArgs.PlcMessage = _lastSentPlcMessage;
                        DriverNotification?.Invoke(this, _driverNotificationEventArgs);
                    }
                    else
                    {
                        logMsg =
                            $"Unexpected counter in ACKN: " +
                            $"Is=[{t.Counter}], ShouldBe=[{_lastSentTelegram.Counter}]";
                        _logger.LogError(logMsg);
                        _logger.LogError(t.HexaDump());
                    }
                }
                else
                {
                    logMsg =
                        $"No pending ACKN expected !";
                    _logger.LogError(logMsg);
                    _logger.LogError(t.HexaDump());
                }
                break;
            }

            _ackTelegram.Sender = t.Receiver;
            _ackTelegram.Receiver = t.Sender;
            _ackTelegram.Identifier = PlcMessageIdentifiers.ACKN.ToString();
            _ackTelegram.AckFlag = "0";
            _ackTelegram.Counter = t.Counter;
            _ackTelegram.Data = t.Data;
            await SendToGLogWareAsync(_ackTelegram, false);

            if (t.Counter == _lastReceivedCounter && t.Counter != "0")
            {
                logMsg =
                    $"Same counter [{t.Counter}] as previous telegram. " +
                    $"It is a retry telegram --> No processing";
                _logger.LogError(logMsg);
                _logger.LogError(t.HexaDump());
                break;
            }
            _lastReceivedCounter = t.Counter;

            PlcMessage plcMessage = new PlcMessage();
            plcMessage.Sender = t.Sender;
            plcMessage.Receiver = t.Receiver;
            switch (t.Identifier)
            {
                case nameof(PlcMessageIdentifiers.ORDS):
                    plcMessage.Identifier = PlcMessageIdentifiers.ORDS;
                    ORDSStruct ordsStruct = ORDSStruct.FromData(t.Data);
                    ORDS ords = ordsStruct.ToMessage(t.Sender);
                    plcMessage.Data = ords;
                    break;
                default:
                    plcMessage.Data = null;
                    break;
            }

            _driverNotificationEventArgs = new DriverNotificationEventArgs();
            _driverNotificationEventArgs.NotificationType = DriverNotificationType.TelegramReceived;
            _driverNotificationEventArgs.PlcMessage = plcMessage;
            DriverNotification?.Invoke(this, _driverNotificationEventArgs);
            
            break;
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private bool ValidateTelegram(LegacyPlcTelegram t)
    {        
        _logger.LogInformation(LogMessages.EnterMethod);

        byte b;
        string errorText = string.Empty;
        bool rValue = false;
        while (true)
        {
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
            if (b != LegacyPlcTelegramConstants.STX)
            {
                errorText =
                    $"Telegramm has wrong start byte: " +
                    $"STX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]";
                break; ;
            }

            b = t.Bytes[^1];
            if (b != LegacyPlcTelegramConstants.ETX)
            {
                errorText =
                    $"Telegramm has wrong end byte: " +
                    $"STX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]";
                break;
            }

            if (!"0|1".Split('|').Contains(t.AckFlag))
            {
                errorText =
                    $"Telegram has invalid AckFlag=[{t.AckFlag}]. " +
                    $"Expected values are: [0]=Acknowledge not required, [1]=Acknowledge required";
                break;
            }

            if (!char.IsDigit(t.Counter[0]))
            {
                errorText =
                    $"Telegram has invalid Counter=[{t.Counter}]";
                break;
            }

            if (t.Receiver != _op)
            {
                errorText =
                    $"Telegram has an invalid Receiver. " +
                    $"(Is=[{t.Receiver}]) != (Should=[{_op}]";
                break;
            }

            if (t.Sender != LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER)
            {
                errorText =
                    $"Telegram has an invalid Sender. " +
                    $"(Is=[{t.Sender}]) != (Should=[{LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER}])";
                break;
            }

            if (_validIdentifiers != string.Empty)
            {
                if (!_validIdentifiers.Split('|').Contains(t.Identifier))
                {
                    errorText =
                        $"Telegram has an invalid Identifier. " +
                        $"(Is=[{t.Identifier}]) != (Should=[{_validIdentifiers}])";
                    break;
                }
            }

            rValue = true;
            break;
        }
        if (!rValue)
        {
            _logger.LogError(errorText);
        }


        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task SendToGLogWareAsync(LegacyPlcTelegram t, bool isNew = false)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        try
        {
            if (isNew)
            {
                await _semaphoreSend.WaitAsync();
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
                t.Build();
                _lastSentTelegram = t;
                _watchdogRetry.Start();
            }
            else if (t.Identifier == PlcMessageIdentifiers.ACKN.ToString())
            {
                t.Build();
            }

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

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async void OnWatchdogRetryAsync(object source, ElapsedEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await SendToGLogWareAsync(_lastSentTelegram, false);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }
    #endregion
}