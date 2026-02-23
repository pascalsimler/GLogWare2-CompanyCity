using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Gudel.GLogWare.BridgeDriver;

public class PlcCommunication : IAsyncDisposable
{
    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    #endregion

    #region Private members
    private string _ipPlc { get; set; } = "127.0.0.1";
    private int _portPlc { get; set; } = 7000;
    private int _delayConnection { get; set; } = 5000;
    private int _delayRetry { get; set; } = 5000;
    private CancellationTokenSource? _cts;
    private TcpClient? _tcpClient;
    #endregion

    //#region Events
    //public event EventHandler<TelegramReceivedEventArgs>? telegramReceived = null;
    //public event EventHandler<SendingReleasedEventArgs>? sendingReleased = null;
    //public event EventHandler<ConnectionStateChangedEventArgs>? connectionStateChanged = null;
    //#endregion

    public PlcCommunication(
        ILogger<Worker> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Entering ...");

        string configPath = $"GantryBridges:{Worker.OP}";
        _ipPlc = _configuration[$"{configPath}:IP"] ?? _ipPlc;
        _logger.LogInformation($"IPAdress=[{_ipPlc}]");
        if (int.TryParse(_configuration[$"{configPath}:Port"], out int tmpPort)) _portPlc = tmpPort;
        _logger.LogInformation($"Port=[{_portPlc}]");
        if (int.TryParse(_configuration[$"{configPath}:DelayConnection"], out int tmpDelayConnection)) _delayConnection = tmpDelayConnection;
        _logger.LogInformation($"DelayConnection=[{_delayConnection}]");
        if (int.TryParse(_configuration[$"{configPath}:DelayRetry"], out int tmpDelayRetry)) _delayRetry = tmpDelayRetry;
        _logger.LogInformation($"DelayRetry=[{_delayRetry}]");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = ConnectLoopAsync(_cts.Token);

        _logger.LogInformation($"Leaving ...");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Entering ...");
        _logger.LogInformation($"Leaving ...");
        await Task.CompletedTask;
    }

    private async Task ConnectLoopAsync(CancellationToken token)
    {
        _logger.LogInformation($"Entering ...");

        while (!token.IsCancellationRequested)
        {
            try
            {
                _tcpClient = new TcpClient();

                _logger.LogInformation($"Connecting to {_ipPlc}:{_portPlc} ...");

                await _tcpClient.ConnectAsync(_ipPlc, _portPlc, token);

                _logger.LogInformation($"Connected !");

                using NetworkStream stream = _tcpClient.GetStream();

                await ReceiveLoopAsync(stream, token);

                _logger.LogWarning($"Connection closed by the PLC !");
            }
            catch (OperationCanceledException)
            {
                break; // normal termination
            }
            catch (SocketException ex)
            {
                _logger.LogWarning(ex, $"Socket error (Network or PLC inaccessible) !");
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, $"Connection interrupted !");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error !");
            }

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

        _logger.LogInformation($"Leaving ...");
        await Task.CompletedTask;
    }

    private async Task ReceiveLoopAsync(NetworkStream stream, CancellationToken token)
    {
        int bytesRead = 0;
        int offset = 0;
        PlcTelegram plcTelegram; 

        _logger.LogInformation($"Entering ...");

        try
        {
            while (true)
            {
                offset = 0;
                plcTelegram = new PlcTelegram(_logger);
                while (offset < plcTelegram.Bytes.Length)
                {
                    bytesRead = await stream.ReadAsync(plcTelegram.Bytes, offset, plcTelegram.Bytes.Length - offset, token);
                    if (bytesRead == 0) break; // connection closed properly
                    offset += bytesRead;
                }
                if (bytesRead == 0)
                {
                    break;
                }

                bool isOk = plcTelegram.Validate();
                _logger.LogInformation($"pclTelegram.Telegram=[{plcTelegram.Telegram}]");
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

        _logger.LogInformation($"Leaving ...");
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Dispose();
        if (_tcpClient != null)
        {
            _tcpClient.Dispose();
        }
    }
}