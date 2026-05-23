namespace Gudel.GLogWare.Shared;

public interface IPlcDriver
{
    void LoadConfiguration(string op, string path);
    Task StartAsync(CancellationToken cancellationToken);
    Task SendAsync(PlcMessage plcMessage);

    event EventHandler<PlcMessageAcknowledgedEventArgs>? MessageAcknowledged;
    event EventHandler<PlcMessageReceivedEventArgs>? MessageReceived;
}