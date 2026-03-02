using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.Shared;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Timers;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager
{
    #region Private members
    private string _plcIp { get; set; } = "127.0.0.1";
    private int _plcPort { get; set; } = 7000;
    private int _plcDelayConnection { get; set; } = 5000;
    private int _plcDelayRetry { get; set; } = 5000;
    private TcpClient? _tcpClient = null;
    private string _lastReceivedCounter = "0";
    private PlcTelegram _lastSentTelegram = null!;
    private PlcTelegram _ackTelegram = null!;
    private System.Timers.Timer _watchdogRetry = null!;
    private LogPlc _lpReceive = null!;
    #endregion region

    #region Initialisation
    private void LoadConfiguration_Plc()
    {
        string path = $"GantryBridges:{OP}";
        _plcIp = _configuration[$"{path}:Ip"] ?? _plcIp;
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpPlcPort)) _plcPort = tmpPlcPort;
        if (int.TryParse(_configuration[$"{path}:DelayConnection"], out int tmpPlcDelayConnection)) _plcDelayConnection = tmpPlcDelayConnection;
        if (int.TryParse(_configuration[$"{path}:DelayRetry"], out int tmpPlcDelayRetry)) _plcDelayRetry = tmpPlcDelayRetry;
        _logger.LogInformation($"_plcIp=[{_plcIp}]");
        _logger.LogInformation($"_plcPort=[{_plcPort}]");
        _logger.LogInformation($"_plcDelayConnectionPlc=[{_plcDelayConnection}]");
        _logger.LogInformation($"_plcDelayRetry=[{_plcDelayRetry}]");
    }

    private void InitLogPlc(LogPlc lp)
    {
        lp.Category = LogPlcCategoryIdentifiers.GANTRY.ToString();
        lp.Process = ServiceName;
    }
    #endregion

    #region Receive
    private async Task TcpConnectLoopAsync(CancellationToken token)
    {
        string information = string.Empty;

        _lastSentTelegram = new PlcTelegram();
        _ackTelegram = new PlcTelegram();
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
                    lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
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
                    lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
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
                lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
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
                lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
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
                lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
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
                lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
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
        PlcTelegram t = new PlcTelegram();

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
            lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
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
            lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
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
            lp.Direction = LogPlcDirectionIdentifiers.GENERAL.ToString();
            lp.Information = information;
            lp.Data = $"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}";
            await _dbLoggerService.WriteLogPlcAsync(lp);
        }

        await Task.CompletedTask;
    }

    private async Task ProcessTelegram(PlcTelegram t)
    {
        _lpReceive = new LogPlc();
        InitLogPlc(_lpReceive);
        _lpReceive.Direction = LogPlcDirectionIdentifiers.PLC_TO_GLOGWARE.ToString();

        if (!Validate(t))
        {
            _lpReceive.Data = t.HexaDump();
            _logger.LogWarning(_lpReceive.Data);
            await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
            return;
        }

        _logger.LogInformation(t.AsciiString);
        if (t.Identifier == PlcMessageIdentifiers.ACKN.ToString())
        {
            if (_watchdogRetry.Enabled)
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
                _lpReceive.Information =
                    $"No pending ACKN expected !";
                _lpReceive.Data = t.HexaDump();
                _logger.LogError(_lpReceive.Information);
                await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
            }
            return;
        }

        _ackTelegram.Sender = t.Receiver;
        _ackTelegram.Receiver = t.Sender;
        _ackTelegram.Identifier = PlcMessageIdentifiers.ACKN.ToString();
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
            return;
        }
        _lastReceivedCounter = t.Counter;

        switch (t.Identifier)
        {
            case nameof(PlcMessageIdentifiers.STAT):
                await Handle_STAT(t);
                break;
            case nameof(PlcMessageIdentifiers.COMP):
                await Handle_COMP(t);
                break;
            case nameof(PlcMessageIdentifiers.ALRM):
                await Handle_ALRM(t);
                break;
        }
    }

    private bool Validate(PlcTelegram t)
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
        _lpReceive.Counter = t.Counter;
        _lpReceive.Sender = t.Sender;
        _lpReceive.Receiver = t.Receiver;
        _lpReceive.Identifier = t.Identifier;

        b = t.Bytes[0];
        if (b != PlcTelegramConstants.STX)
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
        if (b != PlcTelegramConstants.ETX)
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

        if (t.Receiver != PlcTelegramConstants.GLOGWARE_IDENTIFIER)
        {
            _lpReceive.Information =
                $"Telegram has an invalid Receiver. " +
                $"(Is=[{t.Receiver}]) != (Should=[{PlcTelegramConstants.GLOGWARE_IDENTIFIER}]";
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

        string validIdentifiers = @"\b(ACKN|STAT|ALRM|COMP)\b";
        if (!Regex.IsMatch(t.Identifier, validIdentifiers)) 
        {
            _lpReceive.Information =
                $"Telegram has an invalid Identifier. " +
                $"(Is=[{t.Identifier}]) != (Should=[{validIdentifiers}])";
            _logger.LogError(_lpReceive.Information);
            return false;
        }

        return true;
    }

    private async Task Handle_STAT(PlcTelegram t)
    {
    }

    private async Task Handle_COMP(PlcTelegram t)
    {
    }

    private async Task Handle_ALRM(PlcTelegram t)
    {
    }
    #endregion

    #region Send
    private async void OnWatchdogRetry(object source, ElapsedEventArgs e)
    {
        _watchdogRetry.Enabled = false;
        await SendToPlc(_lastSentTelegram, false);
        _watchdogRetry.Enabled = true;
    }

    private async Task SendPlcMessage(PlcMessage pm)
    {
        switch (pm.Identifier)
        {
            case PlcMessageIdentifiers.LIFE:
                await SendToPlcMessage_LIFE(pm);
                break;

            case PlcMessageIdentifiers.ORDS:
                await SendToPlcMessage_ORDS(pm);
                break;
        }
    }

    private async Task SendToPlcMessage_LIFE(PlcMessage pm)
    {
        PlcTelegram t = new PlcTelegram();
        t.Identifier = PlcMessageIdentifiers.LIFE.ToString();
        t.Sender = PlcTelegramConstants.GLOGWARE_IDENTIFIER;
        t.Receiver = OP;
        t.Data = string.Empty;
        await SendToPlc(t, true);
    }

    private async Task SendToPlcMessage_ORDS(PlcMessage pm)
    {
        ORDS ords = GLogWareMessage.DeSerialize<ORDS>(pm.Data!.ToString()!)!;
        (ORDSStruct ordsStruct, string logMsg) = ORDSStruct.FromORDS(ords);

        PlcTelegram t = new PlcTelegram();
        t.Identifier = PlcMessageIdentifiers.ORDS.ToString();
        t.Sender = PlcTelegramConstants.GLOGWARE_IDENTIFIER;
        t.Receiver = OP!;
        t.Data = ordsStruct.ToData();
        t.LogMsg = logMsg;
        await SendToPlc(t, true); 
    }

    private async Task SendToPlc(PlcTelegram t, bool isNew = false)
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
                t.Build();
                _lastSentTelegram = t;
                _watchdogRetry.Enabled = true;
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
                        InitLogPlc(lpSend);
                        lpSend.Direction = LogPlcDirectionIdentifiers.GLOGWARE_TO_PLC.ToString();
                        if (!isNew) lpSend.Information = "Retry !";
                        lpSend.Ackflag = t.AckFlag;
                        lpSend.Counter = t.Counter;
                        lpSend.Sender = t.Sender;
                        lpSend.Receiver = OP;
                        lpSend.Identifier = t.Identifier;
                        lpSend.Data = t.LogMsg;
                        await _dbLoggerService.WriteLogPlcAsync(lpSend);
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

        if (isNew)
        {
           
        }
    }
    #endregion
}