using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.Services.BridgeManager;

public partial class BridgeManager
{
    #region Private members
    private DriverNotificationType _driverState;
    #endregion region

    private void LoadPlcConfiguration()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _plcDriver.LoadConfiguration($"GantryBridges:{OP}");
    }

    private async Task StartPlcDriverAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        
        _plcDriver.DriverNotification += OnPlcDriverNotification;
        await _plcDriver.StartAsync(cancellationToken);
    }

    private async void OnPlcDriverNotification(object? sender, DriverNotificationEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"notificationType=[{e.notificationType}]");

        if (e.notificationType == DriverNotificationType.TelegramReceived)
        {
            await ProcessPlcMessage(e.plcMessage);
        }
        else
        {
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

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task ProcessPlcMessage(PlcMessage pm)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await Lock();
        try
        {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GLogWareMessage");
        }
        Unlock();
    }

}