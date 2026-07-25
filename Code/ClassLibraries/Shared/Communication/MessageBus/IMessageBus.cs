namespace Gudel.GLogWare.MessageBus;

public interface IMessageBus
{
    void LoadConfiguration(string clientId, string[] subscriptionTopics);
    Task StartAsync();
    Task PublishAsync(string topic, string message);
    event EventHandler<MessageBusNotificationEventArgs>? MessageBusNotification;
}