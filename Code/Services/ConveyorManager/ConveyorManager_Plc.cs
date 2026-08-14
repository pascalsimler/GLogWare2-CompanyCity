using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Gudel.GLogWare.PlcDriver;

namespace Gudel.GLogWare.Services.ConveyorManager;

public partial class ConveyorManager
{
    #region Private members
    private DriverNotificationType _driverState;
    #endregion region

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
                    _logger.LogInformation("PLC is now ONLINE");
                    break;
                case DriverNotificationType.Offline:
                    _logger.LogInformation("PLC is now OFFLINE");
                    break;
                case DriverNotificationType.TelegramSent:
                    _logger.LogInformation("PLC has a telegram to send");
                    break;
                case DriverNotificationType.TelegramSentAcknowledged:
                    _logger.LogInformation("PLC acknowledged the sent telegram");
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
                    STATConveyor stat = (STATConveyor)pm.Data!;
                    await Process_STAT(stat);
                    break;
                case PlcMessageIdentifiers.ARIV:
                    ARIV ariv = (ARIV)pm.Data!;
                    await Process_ARIV(ariv);
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
}