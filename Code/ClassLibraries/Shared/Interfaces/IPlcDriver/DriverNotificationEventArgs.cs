namespace Gudel.GLogWare.Shared;

public class DriverNotificationEventArgs : EventArgs
{
    public DriverNotificationType notificationType{ get; set; }
    public PlcMessage plcMessage { get; set; } = null!;
}

public enum DriverNotificationType
{
    Online,
    Offline,
    TelegramReceived,
    TelegramSent,
    TelegramSentAcknowledged
}