namespace Gudel.GLogWare.Shared;

public interface IPlcDriver
{
    void LoadConfiguration(string path);
    Task StartAsync(CancellationToken cancellationToken);
    Task SendAsync(PlcMessage plcMessage);

    event EventHandler<DriverNotificationEventArgs>? DriverNotification;
}