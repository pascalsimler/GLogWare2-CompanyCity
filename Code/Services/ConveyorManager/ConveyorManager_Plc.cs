using Gudel.GLogWare.Interfaces;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.Services.ConveyorManager;

public partial class ConveyorManager
{
    #region Private members
    private DriverNotificationType _driverState;
    #endregion region

    private void LoadPlcConfiguration()
    {
        logger.EnterMethod();

        plcDriver.LoadConfiguration(_configPath);

        logger.LeaveMethod();
    }

    private async Task StartPlcDriverAsync(CancellationToken cancellationToken)
    {
        logger.EnterMethod();
        
        plcDriver.DriverNotification += OnPlcDriverNotification;
        await plcDriver.StartAsync(cancellationToken);

        logger.LeaveMethod();
    }

    private async void OnPlcDriverNotification(object? sender, DriverNotificationEventArgs e)
    {
        logger.EnterMethod();
        logger.LogKeyValue("notificationType", e.NotificationType);

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
                    logger.LogInformation("PLC is now ONLINE");
                    break;
                case DriverNotificationType.Offline:
                    logger.LogInformation("PLC is now OFFLINE");
                    break;
                case DriverNotificationType.TelegramSent:
                    logger.LogInformation("PLC has a telegram to send");
                    break;
                case DriverNotificationType.TelegramSentAcknowledged:
                    logger.LogInformation("PLC acknowledged the sent telegram");
                    break;
            }
        }

        logger.LeaveMethod();
    }

    private async Task ProcessPlcMessage(PlcMessage pm)
    {
        logger.EnterMethod();

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
            logger.LogError(ex, "Error processing GLogWareMessage");
        }
        finally
        {
            Unlock();
        }

        logger.LeaveMethod();
    }
}