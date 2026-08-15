using Gudel.GLogWare.Entities;
using Gudel.GLogWare.Infrastructure;
using Gudel.GLogWare.Interfaces;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Timers;

namespace Gudel.GLogWare.LegacyPlcDriver;

public class LegacyPlcDriver(
    ILogger<LegacyPlcDriver> logger,
    IConfiguration configuration,
    IDbContextFactory<GLogWareDbContext> dbContextFactory
) : IPlcDriver
{
    #region Event handlers
    public event EventHandler<DriverNotificationEventArgs>? DriverNotification;
    #endregion

    #region Driver parameters
    private string _op = string.Empty;
    private string _ip = "127.0.0.1";
    private int _port = 7000;
    private int _delayConnection = 5000;
    private int _delayRetry = 5000;
    private int _delayLife = 30000;
    private string _validSenders = string.Empty;
    private string _validIdentifiers = string.Empty;
    #endregion

    #region Private members
    private TcpClient? _tcpClient = null;
    private string _lastReceivedCounter = "0";
    private LegacyPlcTelegram _lastSentTelegram = null!;
    private PlcMessage _lastSentPlcMessage = null!;
    private LegacyPlcTelegram _ackTelegram = null!;
    private System.Timers.Timer _watchdogRetry = null!;
    private LogPlc _lpReceive = null!;
    private SemaphoreSlim _semaphoreSend = null!;
    private DriverNotificationEventArgs _driverNotificationEventArgs = null!;
    #endregion region

    #region Public methods
    public void LoadConfiguration(string configPath)
    {
        logger.EnterMethod();
        logger.LogKeyValue("configPath", configPath);

        _op = configPath[(configPath.LastIndexOf(':') + 1)..];
        _ip = configuration[$"{configPath}:Ip"] ?? _ip;
        if (int.TryParse(configuration[$"{configPath}:Port"], out int tmpPort)) _port = tmpPort;
        if (int.TryParse(configuration[$"{configPath}:DelayConnection"], out int tmpDelayConnection)) _delayConnection = tmpDelayConnection;
        if (int.TryParse(configuration[$"{configPath}:DelayRetry"], out int tmpDelayRetry)) _delayRetry = tmpDelayRetry;
        if (int.TryParse(configuration[$"{configPath}:DelayLife"], out int tmpDelayLife)) _delayLife = tmpDelayLife;
        _validIdentifiers = configuration[$"{configPath}:ValidPlcIdentifiers"] ?? string.Empty;
        _validSenders = configuration[$"{configPath}:ValidSenders"] ?? string.Empty;

        logger.LogKeyValue("_op", _op);
        logger.LogKeyValue("_ip", _ip);
        logger.LogKeyValue("_port", _port);
        logger.LogKeyValue("_delayConnectionPlc", _delayConnection);
        logger.LogKeyValue("_delayRetry", _delayRetry);
        logger.LogKeyValue("_delayLife", _delayLife);
        logger.LogKeyValue("_validIdentifiers", _validIdentifiers);
        logger.LogKeyValue("_validSenders", _validSenders);

        logger.LeaveMethod();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.EnterMethod();

        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = TcpConnectLoopAsync(cts.Token);

        await Task.CompletedTask;

        logger.LeaveMethod();
    }

    public async Task SendAsync(PlcMessage plcMessage)
    {
        logger.EnterMethod();

        LegacyPlcTelegram t = new()
        {
            Identifier = plcMessage.Identifier.ToString(),
            Sender = "GLOGWARE",
            Receiver = plcMessage.Receiver
        };
        switch (plcMessage.Identifier)
        {
            case PlcMessageIdentifiers.LIFE:
                t.Data = string.Empty;
                break;
            case PlcMessageIdentifiers.ORDS:
                ORDS ords = GLogWareMessage.DeSerialize<ORDS>(plcMessage.Data!.ToString()!)!;
                ORDSStruct ordsStruct = ORDSStruct.FromMessage(ords);
                t.Data = ordsStruct.ToData();
                t.LogMsg = ordsStruct.ToLogMessage(t.Receiver);
                break;
            default:
                t.Data = string.Empty;
                break;
        }
        await SendToPlcAsync(t, true);
        _lastSentPlcMessage = plcMessage;
        _driverNotificationEventArgs = new DriverNotificationEventArgs()
        {
            NotificationType = DriverNotificationType.TelegramSent,
            PlcMessage = _lastSentPlcMessage
        };
        DriverNotification?.Invoke(this, _driverNotificationEventArgs);

        logger.LeaveMethod();
    }
    #endregion

    #region Private method
    private async Task TcpConnectLoopAsync(CancellationToken token)
    {
        logger  .EnterMethod();

        string information = string.Empty;
        
        _lastSentTelegram = new();
        _ackTelegram = new();

        _semaphoreSend = new(1);

        _watchdogRetry = new(_delayRetry);
        _watchdogRetry.Elapsed += OnWatchdogRetryAsync!;
        _watchdogRetry.AutoReset = true;
        _watchdogRetry.Stop();

        while (!token.IsCancellationRequested)
        {
            try
            {
                _tcpClient = new();

                logger.LogInformation("Connecting to {Ip}:{Port} ...", _ip, _port);
                await _tcpClient.ConnectAsync(_ip, _port, token);
                information = $"Connected to {_ip}:{_port} !";
                logger.LogInformation("{Information}", information);
                {
                    LogPlc lp = new()
                    {
                        Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                        Information = information
                    };
                    await WriteLogPlcAsync(lp);
                }
                _driverNotificationEventArgs = new()
                {
                    NotificationType = DriverNotificationType.Online
                };
                DriverNotification?.Invoke(this, _driverNotificationEventArgs);

                using NetworkStream stream = _tcpClient.GetStream();
                await TcpReceiveLoopAsync(stream, token);

                information = "Connection closed by the PLC !";
                logger.LogWarning("{Information}", information);
                {
                    LogPlc lp = new()
                    {
                        Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                        Information = information
                    };
                    await WriteLogPlcAsync(lp);
                }
            }
            catch (OperationCanceledException ex)
            {
                information = "Normal termination";
                logger.LogWarning(ex, "{Information}", information);
                {
                    LogPlc lp = new()
                    {
                        Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                        Information = information,
                        Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}"
                    };
                    await WriteLogPlcAsync(lp);
                }
                break;
            }
            catch (SocketException ex)
            {
                information = "Socket error (Network or PLC inaccessible) !";
                logger.LogWarning(ex, "{Information}", information);
                {
                    LogPlc lp = new()
                    { 
                        Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                        Information = information,
                        Data = $"{ex.Source} ({ex.NativeErrorCode}): {ex.Message}\r\n{ex.StackTrace}"
                    };
                    await WriteLogPlcAsync(lp);
                }
            }
            catch (IOException ex)
            {
                information = "Connection interrupted !";
                logger.LogWarning(ex, "{Information}", information);
                {
                    LogPlc lp = new()
                    {
                        Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                        Information = information,
                        Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}"
                    };
                    await WriteLogPlcAsync(lp);
                }
            }
            catch (Exception ex)
            {
                information = "Unexpected error !";
                logger.LogError(ex, "{Information}", information);
                {
                    LogPlc lp = new()
                    {
                        Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                        Information = information,
                        Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}"
                    };
                    await WriteLogPlcAsync(lp);
                }
            }
            _driverNotificationEventArgs = new()
            { 
                NotificationType = DriverNotificationType.Offline
            };
            DriverNotification?.Invoke(this, _driverNotificationEventArgs);

            if (!token.IsCancellationRequested)
            {
                logger.LogInformation("Reconnecting in {DelayConnection} milliseconds ...", _delayConnection);
                await Task.Delay(TimeSpan.FromMilliseconds(_delayConnection), token);
            }

            _tcpClient?.Dispose();
        }

        await Task.CompletedTask;

        logger.LeaveMethod();
    }

    private async Task TcpReceiveLoopAsync(NetworkStream stream, CancellationToken token)
    {
        int bytesRead = 0;
        int offset;
        string information;
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
                if (bytesRead == 0)
                {
                    information = "Connection closed by the PLC !";
                    logger.LogWarning("{Information}", information);
                    break;
                }

                await ProcessTelegramAsync(t);
            }
        }
        catch (OperationCanceledException ex)
        {
            information = "Normal termination";
            logger.LogWarning(ex, "{Information}", information);
            LogPlc lp = new()
            {
                Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                Information = information,
                Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}"
            };
            await WriteLogPlcAsync(lp);
        }
        catch (IOException ex)
        {
            information = "Connection interrupted !";
            logger.LogWarning(ex, "{Information}", information);
            LogPlc lp = new()
            {
                Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                Information = information,
                Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}"
            };
            await WriteLogPlcAsync(lp);
        }
        catch (Exception ex)
        {
            information = "Unexpected error !";
            logger.LogError(ex, "{Information}", information);
            LogPlc lp = new()
            {
                Direction = LogPlcDirectionIdentifiers.GENERAL.ToString(),
                Information = information,
                Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}"
            };
            await WriteLogPlcAsync(lp);
        }

        await Task.CompletedTask;

        logger.LeaveMethod();
    }

    private async Task ProcessTelegramAsync(LegacyPlcTelegram t)
    {
        logger.EnterMethod();

        _lpReceive = new()
        {
            Direction = LogPlcDirectionIdentifiers.PLC_TO_GLOGWARE.ToString()
        };

        while (true)
        {
            if (!ValidateTelegram(t))
            {
                _lpReceive.Data = t.HexaDump();
                logger.LogWarning("{Information}", _lpReceive.Data);
                await WriteLogPlcAsync(_lpReceive);
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
                        _lpReceive.Information =
                            $"Unexpected counter in ACKN: " +
                            $"Is=[{t.Counter}], ShouldBe=[{_lastSentTelegram.Counter}]";
                        _lpReceive.Data = t.HexaDump();
                        logger.LogError("{ErrorText}", _lpReceive.Information);
                        await WriteLogPlcAsync(_lpReceive);
                    }
                }
                else
                {
                    _lpReceive.Information =
                        $"No pending ACKN expected !";
                    _lpReceive.Data = t.HexaDump();
                    logger.LogError("{ErrorText}", _lpReceive.Information);
                    await WriteLogPlcAsync(_lpReceive);
                }
                break;
            }

            _ackTelegram.Sender = t.Receiver;
            _ackTelegram.Receiver = t.Sender;
            _ackTelegram.Identifier = PlcMessageIdentifiers.ACKN.ToString();
            _ackTelegram.AckFlag = "0";
            _ackTelegram.Counter = t.Counter;
            _ackTelegram.Data = t.Data;
            await SendToPlcAsync(_ackTelegram, false);

            if (t.Counter == _lastReceivedCounter && t.Counter != "0")
            {
                _lpReceive.Information =
                    $"Same counter [{t.Counter}] as previous telegram. " +
                    $"It is a retry telegram --> No processing";
                _lpReceive.Data = t.HexaDump();
                logger.LogError("{ErrorText}", _lpReceive.Information);
                await WriteLogPlcAsync(_lpReceive);
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
                case nameof(PlcMessageIdentifiers.STAT):
                    plcMessage.Identifier = PlcMessageIdentifiers.STAT;
                    STATBridgeStruct statStruct = STATBridgeStruct.FromData(t.Data);
                    STATBridge stat = statStruct.ToMessage(t.Sender);
                    _lpReceive.Data = statStruct.ToLogMessage(t.Sender);
                    plcMessage.Data = stat;
                    break;
                case nameof(PlcMessageIdentifiers.COMP):
                    plcMessage.Identifier = PlcMessageIdentifiers.COMP;
                    COMPStruct compStruct = COMPStruct.FromData(t.Data);
                    COMP comp = compStruct.ToMessage(t.Sender);
                    _lpReceive.Data = compStruct.ToLogMessage(t.Sender);
                    plcMessage.Data = comp;
                    break;
                //case nameof(PlcMessageIdentifiers.ALRM):
                //    break;
                default:
                    _lpReceive.Data = string.Empty;
                    plcMessage.Data = null;
                    break;
            }

            await WriteLogPlcAsync(_lpReceive);
            
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

            //_logger.LogKeyValue("AsciiString", t.AsciiString);
            //_logger.LogKeyValue("AckFlag", t.AckFlag);
            //_logger.LogKeyValue("Counter", t.Counter);
            //_logger.LogKeyValue("Receiver", t.Receiver);
            //_logger.LogKeyValue("Sender", t.Sender);
            //_logger.LogKeyValue("Identifier", t.Identifier);
            //_logger.LogKeyValue("Data", t.Data);
            //_logger.LogKeyValue("HexaDump", t.HexaDump());

            _lpReceive.Ackflag = t.AckFlag;
            _lpReceive.Counter = t.Counter;
            _lpReceive.Sender = t.Sender;
            _lpReceive.Receiver = t.Receiver;
            _lpReceive.Identifier = t.Identifier;

            b = t.Bytes[0];
            if (b != LegacyPlcTelegramConstants.STX)
            {
                errorText =
                    $"Telegramm has wrong start byte: " +
                    $"STX != [Hexa:0x{b:X2} - Decimal:{b} - ASCII:{(char)b}]";
                break;
            }

            b = t.Bytes[^1];
            if (b != LegacyPlcTelegramConstants.ETX)
            {
                errorText =
                    $"Telegramm has wrong end byte: " +
                    $"ETX != [Hexa:0x{b:X2} - Decimal:{b} - ASCII:{(char)b}]";
                break;
            }

            if (!"0|1".Split("|").Contains(t.AckFlag))
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

            if (t.Receiver != LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER)
            {
                errorText =
                    $"Telegram has an invalid Receiver. " +
                    $"(Is=[{t.Receiver}]) != (Should=[{LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER}]";
                break;
            }

            if (_validSenders != string.Empty)
            {
                if (!_validSenders.Split('|').Contains(t.Sender))
                {
                    errorText =
                        $"Telegram has an invalid Sender. " +
                        $"(Is=[{t.Sender}]) != (Should=[{_validSenders}])";
                    break;
                }
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
            logger.LogError("{ErrorText}", errorText);
            _lpReceive.Information = errorText;
        }

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }

    private async void OnWatchdogRetryAsync(object source, ElapsedEventArgs e)
    {
        logger.EnterMethod();

        await SendToPlcAsync(_lastSentTelegram, false);

        logger.LeaveMethod();
    }

    private async Task SendToPlcAsync(LegacyPlcTelegram t, bool isNew = false)
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
                    logger.LogKeyValue("AsciiString", t.AsciiString);
                    //logger.LogKeyValue("Hexa", t.HexaDump());
                    NetworkStream stream = _tcpClient.GetStream();
                    await stream.WriteAsync(t.Bytes.AsMemory(0, t.Bytes.Length));
                    if (!new[] { PlcMessageIdentifiers.ACKN.ToString() /*, PlcMessageIdentifiers.LIFE.ToString()*/ }.Contains(t.Identifier))
                    {
                        LogPlc lpSend = new()
                        {
                            Direction = LogPlcDirectionIdentifiers.GLOGWARE_TO_PLC.ToString(),
                            Information = isNew ? string.Empty : "Retry !",
                            Ackflag = t.AckFlag,
                            Counter = t.Counter,
                            Sender = t.Sender,
                            Receiver = _op,
                            Identifier = t.Identifier,
                            Data = t.LogMsg
                        };
                        await WriteLogPlcAsync(lpSend);
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

    private async Task WriteLogPlcAsync(LogPlc logPlc)
    {        
        try
        {
            using GLogWareDbContext db = dbContextFactory.CreateDbContext();
            logPlc.Process = _op;
            logPlc.Category = (_op[(_op.Length-2)..]) switch
            {
                "BR" => nameof(LogPlcCategoryIdentifiers.GANTRY),
                "PA" => nameof(LogPlcCategoryIdentifiers.PALLETIZER),
                "AL" => nameof(LogPlcCategoryIdentifiers.CONVEYOR),
                _ => nameof(LogPlcCategoryIdentifiers.UNCATEGORIZED)
            };
            db.LogPlcs.Add(logPlc);
            await db.SaveChangesAsync();
        }
        catch
        {
            // ⚠️ NEVER throw from logging
            // swallow intentionally
        }
    }
    #endregion
}