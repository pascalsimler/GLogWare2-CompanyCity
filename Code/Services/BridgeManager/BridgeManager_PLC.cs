using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager
{
    #region Private members
    private DriverNotificationType _driverState;
    #endregion region

    private void LoadPlcConfiguration()
    {
        _plcDriver.LoadConfiguration($"GantryBridges:{OP}");
    }

    private async Task StartPlcDriverAsync(CancellationToken cancellationToken)
    {
        _plcDriver.DriverNotification += OnPlcDriverNotification;
        await _plcDriver.StartAsync(cancellationToken);
    }

    private async void OnPlcDriverNotification(object? sender, DriverNotificationEventArgs e)
    {
        if (e.notificationType == DriverNotificationType.TelegramReceived)
        {
            await ProcessPlcMessage(e.plcMessage);
            return;
        }

        _driverState = e.notificationType;
        switch (_driverState)
        {
            case DriverNotificationType.Online:
                _logger.LogInformation($"PLC is now ONLINE");
                break;
            case DriverNotificationType.Offline:
                _logger.LogInformation($"PLC is now OFFLINE");
                break;
            case DriverNotificationType.TelegramSent:
                _logger.LogInformation($"PLC has a telegram to send");
                break;
            case DriverNotificationType.TelegramSentAcknowledged:
                _logger.LogInformation($"PLC acknowledged the sent telegram");
                break;
        }
    }

    private async void OnPlcMessageAcknowledged(object? sender, DriverNotificationEventArgs e)
    {
    }

    private async Task ProcessPlcMessage(PlcMessage pm)
    {
        _db = _dbContextFactory.CreateDbContext();
        switch (pm.Identifier)
        {
            case PlcMessageIdentifiers.STAT:
                STATBridge stat = (STATBridge)pm.Data!;
                await Process_STAT(stat);
                break;
            case PlcMessageIdentifiers.COMP:
                COMP comp = (COMP)pm.Data!;
                await Process_COMP(comp);
                break;
            default:
                break;
        }
        _db.Dispose();
    }
}