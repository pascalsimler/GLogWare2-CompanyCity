using Gudel.GLogWare.Interfaces;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Gudel.GLogWare.LegacyPlcDriver;

public class LegacyPlcSimulatorDriver(
    ILogger<LegacyPlcDriver> logger,
    IConfiguration configuration
) : IPlcDriver
{
    #region Driver parameters
    private string _op = string.Empty;
    private int _port = 7000;
    private int _delayRetry = 5000;
    private string _validIdentifiers = string.Empty;
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

    #region Public methods
    public void LoadConfiguration(string path)
    {
        logger.EnterMethod();
        logger.LogKeyValue("path", path);

        _op = path[(path.LastIndexOf(':') + 1)..];
        if (int.TryParse(configuration[$"{path}:Port"], out int tmpPort)) _port = tmpPort;
        if (int.TryParse(configuration[$"{path}:DelayRetry"], out int tmpDelayRetry)) _delayRetry = tmpDelayRetry;
        _validIdentifiers = configuration[$"{path}:ValidGLogWareIdentifiers"] ?? string.Empty;

        logger.LogKeyValue("_op", _op);
        logger.LogKeyValue("_port", _port);
        logger.LogKeyValue("_delayRetry", _delayRetry);
        logger.LogKeyValue("_validIdentifiers", _validIdentifiers);

        logger.LeaveMethod();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.EnterMethod();

        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = TcpAcceptLoopAsync(cts.Token);

        logger.LeaveMethod();
        await Task.CompletedTask;
    }

    public async Task SendAsync(PlcMessage plcMessage)
    {
        logger.EnterMethod();

        LegacyPlcTelegram t = new()
        {
            Identifier = plcMessage.Identifier.ToString(),
            Sender = plcMessage.Sender,
            Receiver = plcMessage.Receiver
        };
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
        _driverNotificationEventArgs = new ()
        { 
            NotificationType = DriverNotificationType.TelegramSent,
            PlcMessage = _lastSentPlcMessage
        };
        DriverNotification?.Invoke(this, _driverNotificationEventArgs);

        logger.LeaveMethod();
    }
    #endregion

    #region Private methods
    private async Task TcpAcceptLoopAsync(CancellationToken token)
    {
        logger.EnterMethod();

        _lastSentTelegram = new LegacyPlcTelegram();
        _ackTelegram = new LegacyPlcTelegram();

        _semaphoreSend = new SemaphoreSlim(1);

        _watchdogRetry = new System.Timers.Timer(_delayRetry);
        _watchdogRetry.Elapsed += OnWatchdogRetryAsync!;
        _watchdogRetry.AutoReset = true;
        _watchdogRetry.Stop();

        TcpListener listener = new(IPAddress.Any, _port);
        listener.Start();
        logger.LogInformation("Listening on port {Port} ...", _port);

        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Waiting for a new incoming connection request!");
                    _tcpClient = await listener.AcceptTcpClientAsync(token);
                    logger.LogInformation("Client connected from {RemoteEndPoint} !", _tcpClient.Client.RemoteEndPoint);
                    _driverNotificationEventArgs = new()
                    {
                        NotificationType = DriverNotificationType.Online
                    };
                    DriverNotification?.Invoke(this, _driverNotificationEventArgs);

                    using NetworkStream stream = _tcpClient.GetStream();
                    await TcpReceiveLoopAsync(stream, token);

                    logger.LogWarning("Connection closed by the client !");
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }
                catch (OperationCanceledException)
                {
                    break; // normal termination
                }
                catch (SocketException ex)
                {
                    logger.LogWarning(ex, "Socket error !");
                }
                catch (IOException ex)
                {
                    logger.LogWarning(ex, "Connection interrupted !");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error !");
                }
                _driverNotificationEventArgs = new()
                { 
                    NotificationType = DriverNotificationType.Offline
                };
                DriverNotification?.Invoke(this, _driverNotificationEventArgs);
            }
        }
        finally
        {
            listener.Stop();
            logger.LogInformation("Listener stopped.");
        }

        await Task.CompletedTask;

        logger.LeaveMethod();
    }

    private async Task TcpReceiveLoopAsync(NetworkStream stream, CancellationToken token)
    {
        int bytesRead = 0;
        int offset;
        LegacyPlcTelegram t = new();

        logger.EnterMethod();

        try
        {
            while (true)
            {
                offset = 0;
                Array.Clear(t.Bytes, 0, t.Bytes.Length);
                while (offset < t.Bytes.Length)
                {
                    bytesRead = await stream.ReadAsync(t.Bytes.AsMemory(offset, t.Bytes.Length - offset), token);
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
            logger.LogWarning(ex, "Connection interrupted (IO)");
        }
        catch (SocketException ex)
        {
            logger.LogWarning(ex, "Socket error");
        }

        logger.LeaveMethod();
        await Task.CompletedTask;
    }

    private async Task ProcessTelegramAsync(LegacyPlcTelegram t)
    {
        logger.EnterMethod();

        string logMsg = string.Empty;

        while (true)
        {
            if (!ValidateTelegram(t))
            {
                logger.LogWarning("{Information}", t.HexaDump());
                break;
            }

            logger.LogInformation("{Information}", t.AsciiString);

            if (t.Identifier == PlcMessageIdentifiers.ACKN.ToString())
            {
                if (_watchdogRetry.Enabled)
                {
                    if (t.Counter == _lastSentTelegram.Counter)
                    {
                        _watchdogRetry.Stop();
                        _semaphoreSend.Release();
                        _driverNotificationEventArgs = new()
                        {
                            NotificationType = DriverNotificationType.TelegramSentAcknowledged,
                            PlcMessage = _lastSentPlcMessage
                        };
                        DriverNotification?.Invoke(this, _driverNotificationEventArgs);
                    }
                    else
                    {
                        logMsg =
                            $"Unexpected counter in ACKN: " +
                            $"Is=[{t.Counter}], ShouldBe=[{_lastSentTelegram.Counter}]";
                        logger.LogError("{ErrorText}", logMsg);
                        logger.LogError("{HexaDump}", t.HexaDump());
                    }
                }
                else
                {
                    logMsg =
                        $"No pending ACKN expected !";
                    logger.LogError("{ErrorText}", logMsg);
                    logger.LogError("{HexaDump}", t.HexaDump());
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
                logger.LogError("{ErrorText}", logMsg);
                logger.LogError("{HexaDump}", t.HexaDump());
                break;
            }
            _lastReceivedCounter = t.Counter;

            PlcMessage plcMessage = new()
            { 
                Sender = t.Sender,
                Receiver = t.Receiver
            };
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

            _driverNotificationEventArgs = new()
            {
                NotificationType = DriverNotificationType.TelegramReceived,
                PlcMessage = plcMessage
            };
            DriverNotification?.Invoke(this, _driverNotificationEventArgs);
            
            break;
        }

        logger.LeaveMethod();
    }

    private bool ValidateTelegram(LegacyPlcTelegram t)
    {        
        logger.EnterMethod();

        byte b;
        string errorText = string.Empty;
        bool rValue = false;
        while (true)
        {
            t.Parse();
            //logger.LogKeyValue("AsciiString",t.AsciiString) ;
            //logger.LogKeyValue("AckFlag",t.AckFlag);
            //logger.LogKeyValue("Counter",t.Counter);
            //logger.LogKeyValue("Receiver",t.Receiver);
            //logger.LogKeyValue("Sender",t.Sender);
            //logger.LogKeyValue("Identifier",t.Identifier);
            //logger.LogKeyValue("Data",t.Data);
            //logger.LogKeyValue("HexaDump",t.HexaDump());

            b = t.Bytes[0];
            if (b != LegacyPlcTelegramConstants.STX)
            {
                errorText =
                    $"Telegramm has wrong start byte: " +
                    $"STX != [Hexa:0x{b:X2} - Decimal:{b} - ASCII:{(char)b}]";
                break; ;
            }

            b = t.Bytes[^1];
            if (b != LegacyPlcTelegramConstants.ETX)
            {
                errorText =
                    $"Telegramm has wrong end byte: " +
                    $"ETX != [Hexa:0x{b:X2} - Decimal:{b} - ASCII:{(char)b}]";
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
            logger.LogError("{ErrorText}",errorText);
        }


        logger.LogKeyValue("rValue",rValue);
        logger.LeaveMethod();
        return rValue;
    }

    private async Task SendToGLogWareAsync(LegacyPlcTelegram t, bool isNew = false)
    {
        logger.EnterMethod();

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
                    logger.LogInformation("{Information}", t.AsciiString);
                    NetworkStream stream = _tcpClient.GetStream();
                    await stream.WriteAsync(t.Bytes.AsMemory(0, t.Bytes.Length));
                    if (isNew)
                    {
                        _lastSentTelegram = t;
                        _watchdogRetry!.Enabled = true;
                    }
                }
                else
                {
                    logger.LogError("_tcpClient is not connected !");
                }
            }
            else
            {
                logger.LogError("_tcpClient is null !");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error !");
        }

        logger.LeaveMethod();
    }

    private async void OnWatchdogRetryAsync(object source, ElapsedEventArgs e)
    {
        logger.EnterMethod();

        await SendToGLogWareAsync(_lastSentTelegram, false);

        logger.LeaveMethod();
    }
    #endregion
}