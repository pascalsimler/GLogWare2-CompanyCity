namespace Gudel.GLogWare.MessageBus;

public class MessageBusNotificationEventArgs : EventArgs
{
    public MessageBusNotificationType NotificationType { get; set; }
    public string Topic { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
}

public enum MessageBusNotificationType
{
    Connected,
    Disconnected,
    MessageReceived
}