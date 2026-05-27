using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Timers;

namespace Gudel.GLogWare.LegacyPlcDriver;

public class LegacyPlcDriver: IPlcDriver
{
    #region Dependency injection members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<GLogWareDbContext> _dbContextFactory;
    private GLogWareDbContext _db = null!;
    #endregion

    #region Event handlers
    public event EventHandler<DriverNotificationEventArgs>? DriverNotification;
    #endregion

    #region Driver parameters
    private string _op = string.Empty;
    private string _ip { get; set; } = "127.0.0.1";
    private int _port { get; set; } = 7000;
    private int _delayConnection { get; set; } = 5000;
    private int _delayRetry { get; set; } = 5000;
    private int _delayLife { get; set; } = 30000;
    private string _validSenders { get; set; } = string.Empty;
    private string _validIdentifiers { get; set; } = string.Empty;
    #endregion

    #region Private members
    private TcpClient? _tcpClient = null;
    private string _lastReceivedCounter = "0";
    private LegacyPlcTelegram _lastSentTelegram = null!;
    private LegacyPlcTelegram _ackTelegram = null!;
    private System.Timers.Timer _watchdogRetry = null!;
    private LogPlc _lpReceive = null!;
    private SemaphoreSlim _semaphoreSend = null!;
    #endregion region

    #region Constructor
    public LegacyPlcDriver(
        ILogger<LegacyPlcDriver> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
    }
    #endregion

    #region Public methods
    public void LoadConfiguration(string path)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"path=[{path}]");

        _op = path.Substring(path.LastIndexOf(':') + 1);
        _ip = _configuration[$"{path}:Ip"] ?? _ip;
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpPort)) _port = tmpPort;
        if (int.TryParse(_configuration[$"{path}:DelayConnection"], out int tmpDelayConnection)) _delayConnection = tmpDelayConnection;
        if (int.TryParse(_configuration[$"{path}:DelayRetry"], out int tmpDelayRetry)) _delayRetry = tmpDelayRetry;
        if (int.TryParse(_configuration[$"{path}:DelayLife"], out int tmpDelayLife)) _delayLife = tmpDelayLife;
        _validIdentifiers = _configuration[$"{path}:ValidPlcIdentifiers"] ?? string.Empty;
        _validSenders = _configuration[$"{path}:ValidSenders"] ?? string.Empty;

        _logger.LogInformation($"_op=[{_op}]");
        _logger.LogInformation($"_ip=[{_ip}]");
        _logger.LogInformation($"_port=[{_port}]");
        _logger.LogInformation($"_delayConnectionPlc=[{_delayConnection}]");
        _logger.LogInformation($"_delayRetry=[{_delayRetry}]");
        _logger.LogInformation($"_delayLife=[{_delayLife}]");
        _logger.LogInformation($"_validIdentifiers=[{_validIdentifiers}]");
        _logger.LogInformation($"_validSenders=[{_validSenders}]");

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = TcpConnectLoopAsync(cts.Token);

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    public async Task SendAsync(PlcMessage plcMessage)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        LegacyPlcTelegram t = new LegacyPlcTelegram();
        t.Identifier = plcMessage.Identifier.ToString();
        t.Sender = "GLOGWARE";
        t.Receiver = plcMessage.Receiver;
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

        _logger.LogInformation(LogMessages.LeaveMethod);
    }
    #endregion

    #region Private method
    private async Task TcpConnectLoopAsync(CancellationToken token)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        DriverNotificationEventArgs driverNotificationEventArgs = null!;

        string information = string.Empty;
        
        _lastSentTelegram = new LegacyPlcTelegram();
        _ackTelegram = new LegacyPlcTelegram();

        _semaphoreSend = new SemaphoreSlim(1);

        _watchdogRetry = new System.Timers.Timer(_delayRetry);
        _watchdogRetry.Elapsed += OnWatchdogRetryAsync!;
        _watchdogRetry.AutoReset = true;
        _watchdogRetry.Stop();

        while (!token.IsCancellationRequested)
        {
            try
            {
                _tcpClient = new TcpClient();

                _logger.LogInformation($"Connecting to {_ip}:{_port} ...");
                await _tcpClient.ConnectAsync(_ip, _port, token);
                information = $"Connected to {_ip}:{_port} !";
                _logger.LogInformation(information);
                {
                    LogPlc lp = new LogPlc();
                    lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
                    lp.Information = information;
                    await WriteLogPlcAsync(lp);
                }
                driverNotificationEventArgs = new DriverNotificationEventArgs();
                driverNotificationEventArgs.notificationType = DriverNotificationType.Online;
                DriverNotification?.Invoke(this, driverNotificationEventArgs);

                using NetworkStream stream = _tcpClient.GetStream();
                await TcpReceiveLoopAsync(stream, token);

                information = $"Connection closed by the PLC !";
                _logger.LogWarning(information);
                {
                    LogPlc lp = new LogPlc();
                    lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
                    lp.Information = information;
                    await WriteLogPlcAsync(lp);
                }
            }
            catch (OperationCanceledException ex)
            {
                information = $"Normal termination";
                _logger.LogWarning(ex, information);
                {
                    LogPlc lp = new LogPlc();
                    lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
                    lp.Information = information;
                    lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
                    await WriteLogPlcAsync(lp);
                }
                break;
            }
            catch (SocketException ex)
            {
                information = $"Socket error (Network or PLC inaccessible) !";
                _logger.LogWarning(ex, information);
                {
                    LogPlc lp = new LogPlc();
                    lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
                    lp.Information = information;
                    lp.Data = $"{ex.Source} ({ex.NativeErrorCode}): {ex.Message}\r\n{ex.StackTrace}";
                    await WriteLogPlcAsync(lp);
                }
            }
            catch (IOException ex)
            {
                information = $"Connection interrupted !";
                _logger.LogWarning(ex, information);
                {
                    LogPlc lp = new LogPlc();
                    lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
                    lp.Information = information;
                    lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
                    await WriteLogPlcAsync(lp);
                }
            }
            catch (Exception ex)
            {
                information = $"Unexpected error !";
                _logger.LogError(ex, information);
                {
                    LogPlc lp = new LogPlc();
                    lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
                    lp.Information = information;
                    lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
                    await WriteLogPlcAsync(lp);
                }
            }
            driverNotificationEventArgs = new DriverNotificationEventArgs();
            driverNotificationEventArgs.notificationType = DriverNotificationType.Offline;
            DriverNotification?.Invoke(this, driverNotificationEventArgs);

            if (!token.IsCancellationRequested)
            {
                _logger.LogInformation($"Reconnecting in {_delayConnection} milliseconds ...");
                await Task.Delay(TimeSpan.FromMilliseconds(_delayConnection), token);
            }

            if (_tcpClient != null)
            {
                _tcpClient.Dispose();
            }
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    private async Task TcpReceiveLoopAsync(NetworkStream stream, CancellationToken token)
    {
        int bytesRead = 0;
        int offset = 0;
        string information = string.Empty;
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
                if (bytesRead == 0)
                {
                    information = $"Connection closed by the PLC !";
                    _logger.LogWarning(information);
                    break;
                }

                await ProcessTelegramAsync(t);
            }
        }
        catch (OperationCanceledException ex)
        {
            information = $"Normal termination";
            _logger.LogWarning(ex, information);
            LogPlc lp = new LogPlc();
            lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
            lp.Information = information;
            lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
            await WriteLogPlcAsync(lp);
        }
        catch (IOException ex)
        {
            information = $"Connection interrupted !";
            _logger.LogWarning(ex, information);
            LogPlc lp = new LogPlc();
            lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
            lp.Information = information;
            lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
            await WriteLogPlcAsync(lp);
        }
        catch (Exception ex)
        {
            information = $"Unexpected error !";
            _logger.LogError(ex, information);
            LogPlc lp = new LogPlc();
            lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
            lp.Information = information;
            lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
            await WriteLogPlcAsync(lp);
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
        await Task.CompletedTask;
    }

    private async Task ProcessTelegramAsync(LegacyPlcTelegram t)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        DriverNotificationEventArgs driverNotificationEventArgs = null!;

        _lpReceive = new LogPlc();
        _lpReceive.Direction = LogPlcDirectionIdentifiers.PLC_TO_GLOGWARE.ToString();

        while (true)
        {
            if (!ValidateTelegram(t))
            {
                _lpReceive.Data = t.HexaDump();
                _logger.LogWarning(_lpReceive.Data);
                await WriteLogPlcAsync(_lpReceive);
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
                        driverNotificationEventArgs = new DriverNotificationEventArgs();
                        driverNotificationEventArgs.notificationType = DriverNotificationType.TelegramSentAcknowledged;
                        DriverNotification?.Invoke(this, driverNotificationEventArgs);
                    }
                    else
                    {
                        _lpReceive.Information =
                            $"Unexpected counter in ACKN: " +
                            $"Is=[{t.Counter}], ShouldBe=[{_lastSentTelegram.Counter}]";
                        _lpReceive.Data = t.HexaDump();
                        _logger.LogError(_lpReceive.Information);
                        await WriteLogPlcAsync(_lpReceive);
                    }
                }
                else
                {
                    _lpReceive.Information =
                        $"No pending ACKN expected !";
                    _lpReceive.Data = t.HexaDump();
                    _logger.LogError(_lpReceive.Information);
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
                _logger.LogError(_lpReceive.Information);
                await WriteLogPlcAsync(_lpReceive);
                break;
            }
            _lastReceivedCounter = t.Counter;

            PlcMessage plcMessage = new PlcMessage();
            plcMessage.Sender = t.Sender;
            plcMessage.Receiver = t.Receiver;
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
            
            driverNotificationEventArgs = new DriverNotificationEventArgs();
            driverNotificationEventArgs.notificationType = DriverNotificationType.TelegramReceived;
            driverNotificationEventArgs.plcMessage = plcMessage;
            DriverNotification?.Invoke(this, driverNotificationEventArgs);

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
                    $"STX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]";
                break;
            }

            b = t.Bytes[^1];
            if (b != LegacyPlcTelegramConstants.ETX)
            {
                errorText =
                    $"Telegramm has wrong end byte: " +
                    $"ETX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]";
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
            _logger.LogError(errorText);
            _lpReceive.Information = errorText;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async void OnWatchdogRetryAsync(object source, ElapsedEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await SendToPlcAsync(_lastSentTelegram, false);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task SendToPlcAsync(LegacyPlcTelegram t, bool isNew = false)
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
                    _logger.LogInformation($"{t.AsciiString}");
                    //_logger.LogInformation($"Hexa: {t.HexaDump()}");
                    NetworkStream stream = _tcpClient.GetStream();
                    await stream.WriteAsync(t.Bytes, 0, t.Bytes.Length);
                    if (!new[] { PlcMessageIdentifiers.ACKN.ToString(), PlcMessageIdentifiers.LIFE.ToString() }.Contains(t.Identifier))
                    {
                        LogPlc lpSend = new LogPlc();
                        lpSend.Direction = LogPlcDirectionIdentifiers.GLOGWARE_TO_PLC.ToString();
                        if (!isNew) lpSend.Information = "Retry !";
                        lpSend.Ackflag = t.AckFlag;
                        lpSend.Counter = t.Counter;
                        lpSend.Sender = t.Sender;
                        lpSend.Receiver = _op;
                        lpSend.Identifier = t.Identifier;
                        lpSend.Data = t.LogMsg;
                        await WriteLogPlcAsync(lpSend);
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

    private async Task WriteLogPlcAsync(LogPlc logPlc)
    {        
        try
        {
            using GLogWareDbContext db = _dbContextFactory.CreateDbContext();
            logPlc.Category = (_op.Substring(_op.Length-2)) switch
            {
                "BR" => nameof(LogPlcCategoryIdentifiers.GANTRY),
                "PA" => nameof(LogPlcCategoryIdentifiers.PALLETIZER),
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