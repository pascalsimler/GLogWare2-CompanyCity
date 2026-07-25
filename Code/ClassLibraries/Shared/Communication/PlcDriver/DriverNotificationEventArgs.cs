using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.PlcDriver;

public class DriverNotificationEventArgs : EventArgs
{
    public DriverNotificationType NotificationType { get; set; }
    public PlcMessage PlcMessage { get; set; } = null!;
}

public enum DriverNotificationType
{
    Online,
    Offline,
    TelegramReceived,
    TelegramSent,
    TelegramSentAcknowledged
}