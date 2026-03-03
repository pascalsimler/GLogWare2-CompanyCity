using Gudel.GLogWare.Shared;
using System.Net;
using System.Net.Sockets;

namespace Gudel.GLogWare.BridgeSimulator;

public partial class BridgeSimulator
{
    #region Private members
    private int _port { get; set; } = 7000;
    private int _delayRetry { get; set; } = 5000;
    private TcpClient? _tcpClient = null;
    private string _lastReceivedCounter = "0";
    private GLogWareTelegram _lastSentTelegram = null!;
    private GLogWareTelegram _ackTelegram = null!;
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

        _lastSentTelegram = new GLogWareTelegram();
        _ackTelegram = new GLogWareTelegram();
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
        GLogWareTelegram t = new GLogWareTelegram();

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

                if (!Validate(t))
                {
                    _logger.LogInformation(t.HexaDump());
                    continue;
                }

                _logger.LogInformation(t.AsciiString);
                if (t.Identifier == PlcMessageIdentifiers.ACKN.ToString())
                {
                    if (t.Counter == _lastSentTelegram.Counter)
                    {
                        _watchdogRetry.Enabled = false;
                        //if (sendingReleased != null)
                        //    sendingReleased.Invoke(this, new SendingReleasedEventArgs());
                    }
                    else
                    {
                        _logger.LogError(
                            $"Unexpected counter in ACKN: Is=[{t.Counter}], ShouldBe=[{_lastSentTelegram.Counter}]");
                    }
                }
                else
                {
                    _ackTelegram.Sender = t.Receiver;
                    _ackTelegram.Receiver = t.Sender;
                    _ackTelegram.Identifier = PlcMessageIdentifiers.ACKN.ToString();
                    _ackTelegram.AckFlag = "0";
                    _ackTelegram.Counter = t.Counter;
                    _ackTelegram.Data = t.Data;
                    await SendToPlc(_ackTelegram, false);
                    if (t.Counter == _lastReceivedCounter && t.Counter != "0")
                    {
                        _logger.LogError($"Same counter as previous telegram --> No processing");
                    }
                    else
                    {
                        _lastReceivedCounter = t.Counter;
                        //if (telegramReceived != null)
                        //    telegramReceived.Invoke(this, new TelegramReceivedEventArgs(_receivedTelegram));
                    }
                }
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

}
