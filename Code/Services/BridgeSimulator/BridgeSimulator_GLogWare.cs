using Gudel.GLogWare.Shared;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Timers;

namespace Gudel.GLogWare.BridgeSimulator;

public partial class BridgeSimulator
{
    #region Private members
    private int _port { get; set; } = 7000;
    private int _delayRetry { get; set; } = 5000;
    private TcpClient? _tcpClient = null;
    private string _lastReceivedCounter = "0";
    private LegacyPlcTelegram _lastSentTelegram = null!;
    private LegacyPlcTelegram _ackTelegram = null!;
    private System.Timers.Timer _watchdogRetry = null!;
    #endregion

    private void LoadConfigurationGLogWare()
    {
        string path = $"GantryBridges:{OP}";
        if (int.TryParse(_configuration[$"{path}:Port"], out int tmpPort)) _port = tmpPort;
        if (int.TryParse(_configuration[$"{path}:DelayRetry"], out int tmpDelayRetry)) _delayRetry = tmpDelayRetry;
        _logger.LogInformation($"_port=[{_port}]");
        _logger.LogInformation($"_delayRetry=[{_delayRetry}]");
    }

    private async Task TcpAcceptLoopAsync(CancellationToken token)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();
        _logger.LogInformation($"Listening on port {_port} ...");

        _lastSentTelegram = new LegacyPlcTelegram();
        _ackTelegram = new LegacyPlcTelegram();
        _watchdogRetry = new System.Timers.Timer(_delayRetry);
        _watchdogRetry.Elapsed += OnWatchdogRetry!;
        _watchdogRetry.AutoReset = true;
        _watchdogRetry.Enabled = false;

        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Waiting for a new incoming connection request!");
                    _tcpClient = await listener.AcceptTcpClientAsync(token);
                    _logger.LogInformation($"Client connected from {_tcpClient.Client.RemoteEndPoint} !");

                    await SendCurrentSTAT();

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
        LegacyPlcTelegram t = new LegacyPlcTelegram();

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

                await ProcessTelegram(t);
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

    private async Task ProcessTelegram(LegacyPlcTelegram t)
    {
        string logMsg = string.Empty;

        if (!Validate(t))
        {
            _logger.LogWarning(t.HexaDump());
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
            return;
        }

        _ackTelegram.Sender = t.Receiver;
        _ackTelegram.Receiver = t.Sender;
        _ackTelegram.Identifier = PlcMessageIdentifiers.ACKN.ToString();
        _ackTelegram.AckFlag = "0";
        _ackTelegram.Counter = t.Counter;
        _ackTelegram.Data = t.Data;
        await SendToGLogWare(_ackTelegram, false);

        if (t.Counter == _lastReceivedCounter && t.Counter != "0")
        {
            logMsg =
                $"Same counter [{t.Counter}] as previous telegram. " +
                $"It is a retry telegram --> No processing";
            _logger.LogError(logMsg);
            _logger.LogError(t.HexaDump());
            return;
        }
        _lastReceivedCounter = t.Counter;

        await ProcessGLogWareTelegram(t);
    }

    private async void OnWatchdogRetry(object source, ElapsedEventArgs e)
    {
        _watchdogRetry.Enabled = false;
        await SendToGLogWare(_lastSentTelegram, false);
        _watchdogRetry.Enabled = true;
    }

    public async Task SendToGLogWare(LegacyPlcTelegram t, bool isNew = false)
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

    private bool Validate(LegacyPlcTelegram t)
    {
        string information = string.Empty;
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
        if (b != LegacyPlcTelegramConstants.STX)
        {
            information =
                $"Telegramm has wrong start byte: " +
                $"STX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]";
            _logger.LogError(information);
            return false;
        }

        b = t.Bytes[^1];
        if (b != LegacyPlcTelegramConstants.ETX)
        {
            information =
                $"Telegramm has wrong end byte: " +
                $"STX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]";
            _logger.LogError(information);
             return false;
        }

        if (!Regex.IsMatch(t.AckFlag, @"^[0-1]$"))
        {
            information =
                $"Telegram has invalid AckFlag=[{t.AckFlag}]. " +
                $"Expected values are: [0]=Acknowledge not required, [1]=Acknowledge required";
            _logger.LogError(information);
            return false;
        }

        if (!Regex.IsMatch(t.Counter, @"^[0-9]$"))
        {
            information =
                $"Telegram has invalid Counter=[{t.Counter}]";
            _logger.LogError(information);
            return false;
        }

        if (t.Receiver != OP)
        {
            information =
                $"Telegram has an invalid Receiver. " +
                $"(Is=[{t.Receiver}]) != (Should=[{OP}]";
            _logger.LogError(information);
            return false;
        }

        if (t.Sender != LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER)
        {
            information =
                $"Telegram has an invalid Sender. " +
                $"(Is=[{t.Sender}]) != (Should=[{LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER}])";
            _logger.LogError(information);
            return false;
        }

        string validIdentifiers = @"\b(ACKN|LIFE|ORDS)\b";
        if (!Regex.IsMatch(t.Identifier, validIdentifiers))
        {
            information =
                $"Telegram has an invalid Identifier. " +
                $"(Is=[{t.Identifier}]) != (Should=[{validIdentifiers}])";
            _logger.LogError(information);
            return false;
        }

        return true;
    }

    private async Task SendTelegram(PlcMessage pm)
    {
        ProcessPlcMessage(pm);
        switch (pm.Identifier)
        {
            case PlcMessageIdentifiers.STAT:
                await SendTelegram_STAT(pm);
                break;

            case PlcMessageIdentifiers.COMP:
                await SendTelegram_COMP(pm);
                break;
        }
    }

    private async Task SendTelegram_STAT(PlcMessage pm)
    {
        STATBridge stat = GLogWareMessage.DeSerialize<STATBridge>(pm.Data!.ToString()!)!;
        STATBridgeStruct statStruct = STATBridgeStruct.FromMessage(stat);

        LegacyPlcTelegram t = new LegacyPlcTelegram();
        t.Identifier = PlcMessageIdentifiers.STAT.ToString();
        t.Receiver = LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER;
        t.Sender = OP!;
        t.Data = statStruct.ToData();

        await SendToGLogWare(t, true);
    }

    private async Task SendTelegram_COMP(PlcMessage pm)
    {
        COMP comp = GLogWareMessage.DeSerialize<COMP>(pm.Data!.ToString()!)!;
        COMPStruct compStruct = COMPStruct.FromMessage(comp);

        LegacyPlcTelegram t = new LegacyPlcTelegram();
        t.Identifier = PlcMessageIdentifiers.COMP.ToString();
        t.Receiver = LegacyPlcTelegramConstants.GLOGWARE_IDENTIFIER;
        t.Sender = OP!;
        t.Data = compStruct.ToData();

        await SendToGLogWare(t, true);
    }
}
