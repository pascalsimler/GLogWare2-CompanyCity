using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Gudel.GLogWare.PlcDriver;

namespace Gudel.GLogWare.Services.BridgeManager;

public partial class BridgeManager
{
    #region Private members
    private DriverNotificationType _driverState = DriverNotificationType.Offline;
    #endregion region

    #region Private methods
    private void LoadPlcConfiguration()
    {
        _logger.EnterMethod();

        _plcDriver.LoadConfiguration(_configPath);

        _logger.LeaveMethod();
    }

    private async Task StartPlcDriverAsync(CancellationToken cancellationToken)
    {
        _logger.EnterMethod();
        
        _plcDriver.DriverNotification += OnPlcDriverNotification;
        await _plcDriver.StartAsync(cancellationToken);

        _logger.LeaveMethod();
    }

    private async void OnPlcDriverNotification(object? sender, DriverNotificationEventArgs e)
    {
        _logger.EnterMethod();
        _logger.LogKeyValue("notificationType", e.NotificationType);

        if (e.NotificationType == DriverNotificationType.TelegramReceived)
        {
            await ProcessPlcMessage(e.PlcMessage);
        }
        else
        {
            _driverState = e.NotificationType;
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

        _logger.LeaveMethod();
    }

    private async Task ProcessPlcMessage(PlcMessage pm)
    {
        _logger.EnterMethod();

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
        finally
        {
            Unlock();
            ResetTimer(_watchdogWakeup);
        }

        _logger.LeaveMethod();
    }
    #endregion
}