using Gudel.GLogWare.Logging;
using Gudel.GLogWare.MessageBus;
using Gudel.GLogWare.Messages;

namespace SimulatorWebApp.Services;

public class MessageBusService
{
    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMessageBus _messageBus;
    #endregion

    public MessageBusService(
        ILogger<MessageBusService> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IMessageBus messageBus)
    {
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _messageBus = messageBus;
    }


    public async Task StartAsync()
    {
        _logger.EnterMethod();

        _messageBus.Init(
              $"Simulator-{GetClientIp()}",
              []
          );

        await _messageBus.StartAsync();
      
        _logger.LeaveMethod();
        await Task.CompletedTask;
    }

    public async Task SendMessage(string topic, GLogWareMessage m)
    {
        string payload;

        try
        {
            m.Sender = "Simulator";
            payload = m.Serialize();

            _logger.LogKeyValue("topic", topic);
            _logger.LogKeyValue("payload", payload);

            await _messageBus.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
        }
    }

    public string? GetClientIp()
    {
        return _httpContextAccessor.HttpContext?
            .Connection
            .RemoteIpAddress?
            .ToString();
    }
}