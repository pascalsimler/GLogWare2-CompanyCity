namespace Gudel.GLogWare.Interfaces;

public interface IMessageBus
{
    void Init(string clientId, string[] subscriptionTopics);
    Task StartAsync();
    Task PublishAsync(string topic, string message);
    event EventHandler<MessageBusNotificationEventArgs>? MessageBusNotification;
}