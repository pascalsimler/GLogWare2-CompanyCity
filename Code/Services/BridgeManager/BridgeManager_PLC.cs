using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.Shared;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Timers;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager
{
    #region Private members
    #endregion region

    private void LoadPlcConfiguration()
    {
        _plcDriver.LoadConfiguration(OP!, $"GantryBridges:{OP}");
    }

    private async Task StartPlcDriverAsync(CancellationToken cancellationToken)
    {
        _plcDriver.MessageReceived += OnPlcMessageReceived;
        _plcDriver.MessageAcknowledged += OnPlcMessageAcknowledged;
        await _plcDriver.StartAsync(cancellationToken);
    }

    public async void OnPlcMessageReceived(object? sender, PlcMessageReceivedEventArgs e)
    {
    }

    public async void OnPlcMessageAcknowledged(object? sender, PlcMessageAcknowledgedEventArgs e)
    {
    }

    private void InitLogPlc(LogPlc lp)
    {
        lp.Category = LogPlcCategoryIdentifiers.GANTRY.ToString();
        lp.Process = ServiceName;
    }


    //private async Task ProcessTelegram(LegacyPlcTelegram t)
    //{
    //    _lpReceive = new LogPlc();
    //    InitLogPlc(_lpReceive);
    //    _lpReceive.Direction = LogPlcDirectionIdentifiers.PLC_TO_GLOGWARE.ToString();

    //    if (!Validate(t))
    //    {
    //        _lpReceive.Data = t.HexaDump();
    //        _logger.LogWarning(_lpReceive.Data);
    //        await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
    //        return;
    //    }

    //    _logger.LogInformation(t.AsciiString);
    //    if (t.Identifier == PlcMessageIdentifiers.ACKN.ToString())
    //    {
    //        if (_watchdogRetry.Enabled)
    //        {
    //            if (t.Counter == _lastSentTelegram.Counter)
    //            {
    //                _watchdogRetry.Stop();
    //                _semaphoreSend.Release();
    //            }
    //            else
    //            {
    //                _lpReceive.Information =
    //                    $"Unexpected counter in ACKN: " +
    //                    $"Is=[{t.Counter}], ShouldBe=[{_lastSentTelegram.Counter}]";
    //                _lpReceive.Data = t.HexaDump();
    //                _logger.LogError(_lpReceive.Information);
    //                await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
    //            }
    //        }
    //        else
    //        {
    //            _lpReceive.Information =
    //                $"No pending ACKN expected !";
    //            _lpReceive.Data = t.HexaDump();
    //            _logger.LogError(_lpReceive.Information);
    //            await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
    //        }
    //        return;
    //    }

    //    _ackTelegram.Sender = t.Receiver;
    //    _ackTelegram.Receiver = t.Sender;
    //    _ackTelegram.Identifier = PlcMessageIdentifiers.ACKN.ToString();
    //    _ackTelegram.AckFlag = "0";
    //    _ackTelegram.Counter = t.Counter;
    //    _ackTelegram.Data = t.Data;
    //    await SendToPlc(_ackTelegram, false);

    //    if (t.Counter == _lastReceivedCounter && t.Counter != "0")
    //    {
    //        _lpReceive.Information =
    //            $"Same counter [{t.Counter}] as previous telegram. " +
    //            $"It is a retry telegram --> No processing";
    //        _lpReceive.Data = t.HexaDump();
    //        _logger.LogError(_lpReceive.Information);
    //        await _dbLoggerService.WriteLogPlcAsync(_lpReceive);
    //        return;
    //    }
    //    _lastReceivedCounter = t.Counter;

    //    await Lock();
    //    try
    //    {
    //        switch (t.Identifier)
    //        {
    //            case nameof(PlcMessageIdentifiers.STAT):
    //                await Handle_STAT(t);
    //                await SendWakeUp(_subscriptionTopic);
    //                break;
    //            case nameof(PlcMessageIdentifiers.COMP):
    //                await Handle_COMP(t);
    //                await SendWakeUp(_subscriptionTopic);
    //                break;
    //            case nameof(PlcMessageIdentifiers.ALRM):
    //                await Handle_ALRM(t);
    //                break;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error handling PLC telegram");
    //    }
    //    Unlock();
    //}

    //private async Task Handle_STAT(LegacyPlcTelegram t)
    //{
    //    STATBridgeStruct statStruct = STATBridgeStruct.FromData(t.Data);
    //    STATBridge stat = statStruct.ToMessage(t.Sender);
    //    _lpReceive.Data = statStruct.ToLogMessage(t.Sender);
    //    await _dbLoggerService.WriteLogPlcAsync(_lpReceive);

    //    await Process_STAT(stat);
    //}

    //private async Task Handle_COMP(LegacyPlcTelegram t)
    //{
    //    COMPStruct compStruct = COMPStruct.FromData(t.Data);
    //    COMP comp = compStruct.ToMessage(t.Sender);
    //    string logMsg = compStruct.ToLogMessage(t.Sender);
    //    _lpReceive.Data = logMsg;
    //    await _dbLoggerService.WriteLogPlcAsync(_lpReceive);

    //    await Process_COMP(comp);
    //}

    //private async Task Handle_ALRM(LegacyPlcTelegram t)
    //{
    //}

    //private async void OnWatchdogRetry(object source, ElapsedEventArgs e)
    //{
    //    _watchdogRetry.Stop();
    //    await SendToPlc(_lastSentTelegram, false);
    //    _watchdogRetry.Start();
    //}

    //private async void OnWatchdogLife(object source, ElapsedEventArgs e)
    //{
    //    if (_watchdogRetry.Enabled) return;
    //    if (_tcpClient == null) return;
    //    if (!_tcpClient.Connected) return;

    //    PlcMessage pm = new PlcMessage();
    //    pm.Identifier = PlcMessageIdentifiers.LIFE;
    //    await SendTelegram(pm);
    //}

    //private async Task SendTelegram(PlcMessage pm)
    //{
    //    switch (pm.Identifier)
    //    {
    //        case PlcMessageIdentifiers.LIFE:
    //            await SendTelegram_LIFE(pm);
    //            break;

    //        case PlcMessageIdentifiers.ORDS:
    //            await SendTelegram_ORDS(pm);
    //            break;
    //    }
    //}

    //private async Task SendTelegram_LIFE(PlcMessage pm)
    //{
    //    LegacyPlcTelegram t = new LegacyPlcTelegram();
    //    t.Identifier = PlcMessageIdentifiers.LIFE.ToString();
    //    t.Sender = LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER;
    //    t.Receiver = OP!;
    //    t.Data = string.Empty;
    //    await SendToPlc(t, true);
    //}

    //private async Task SendTelegram_ORDS(PlcMessage pm)
    //{
    //    ORDS ords = GLogWareMessage.DeSerialize<ORDS>(pm.Data!.ToString()!)!;
    //    ORDSStruct ordsStruct = ORDSStruct.FromMessage(ords);
    //    string logMsg = ordsStruct.ToLogMessage(OP!);

    //    LegacyPlcTelegram t = new LegacyPlcTelegram();
    //    t.Identifier = PlcMessageIdentifiers.ORDS.ToString();
    //    t.Sender = LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER;
    //    t.Receiver = OP!;
    //    t.Data = ordsStruct.ToData();
    //    t.LogMsg = logMsg;
    //    await SendToPlc(t, true); 
    //}

    //private async Task SendToPlc(LegacyPlcTelegram t, bool isNew = false)
    //{
    //    try
    //    {
    //        if (isNew)
    //        {
    //            await _semaphoreSend.WaitAsync();
    //            t.AckFlag = "1";
    //            if (_lastSentTelegram.Counter == string.Empty)
    //            {
    //                t.Counter = "0";
    //            }
    //            else
    //            {
    //                int counter = int.Parse(_lastSentTelegram.Counter);
    //                counter++;
    //                if (counter > 9) counter = 1;
    //                t.Counter = $"{counter:0}";
    //            }
    //            t.Build();
    //            _lastSentTelegram = t;
    //            _watchdogRetry.Start();
    //        }
    //        else if (t.Identifier == PlcMessageIdentifiers.ACKN.ToString())
    //        {
    //            t.Build();
    //        }

    //        if (_tcpClient != null)
    //        {
    //            if (_tcpClient.Connected)
    //            {
    //                _logger.LogInformation($"{t.AsciiString}");
    //                //_logger.LogInformation($"Hexa: {t.HexaDump()}");
    //                NetworkStream stream = _tcpClient.GetStream();
    //                await stream.WriteAsync(t.Bytes, 0, t.Bytes.Length);
    //                if (!new[] { PlcMessageIdentifiers.ACKN.ToString(), PlcMessageIdentifiers.LIFE.ToString() }.Contains(t.Identifier)) 
    //                {
    //                    LogPlc lpSend = new LogPlc();
    //                    InitLogPlc(lpSend);
    //                    lpSend.Direction = LogPlcDirectionIdentifiers.GLOGWARE_TO_PLC.ToString();
    //                    if (!isNew) lpSend.Information = "Retry !";
    //                    lpSend.Ackflag = t.AckFlag;
    //                    lpSend.Counter = t.Counter;
    //                    lpSend.Sender = t.Sender;
    //                    lpSend.Receiver = OP;
    //                    lpSend.Identifier = t.Identifier;
    //                    lpSend.Data = t.LogMsg;
    //                    await _dbLoggerService.WriteLogPlcAsync(lpSend);
    //                    RestartTimer(_watchdogLife);
    //                }
    //            }
    //            else
    //            {
    //                _logger.LogError($"_tcpClient is not connected !");
    //            }
    //        }
    //        else
    //        {
    //            _logger.LogError($"_tcpClient is null !");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, $"Error !");
    //    }
    //}

    //private void RestartTimer(System.Timers.Timer timer)
    //{
    //    timer.Stop();
    //    timer.Start();
    //}
}