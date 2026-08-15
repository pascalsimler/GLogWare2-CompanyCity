using Gudel.GLogWare.Interfaces;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.Services.ConveyorSimulator;

public partial class ConveyorSimulator
{
    #region Private members
    private DriverNotificationType _driverState;
    #endregion region

    private void LoadGLogWareConfiguration()
    {
        logger.EnterMethod();

        plcSimulatorDriver.LoadConfiguration(_configPath);

        logger.LeaveMethod();
    }

    private async Task StartPlcSimulatorDriverAsync(CancellationToken cancellationToken)
    {
        logger.EnterMethod();

        plcSimulatorDriver.DriverNotification += OnPlcSimulatorDriverNotification;
        await plcSimulatorDriver.StartAsync(cancellationToken);

        logger.LeaveMethod();
    }

    private async void OnPlcSimulatorDriverNotification(object? sender, DriverNotificationEventArgs e)
    {
        logger.EnterMethod();

        if (e.NotificationType == DriverNotificationType.TelegramReceived)
        {
            await ProcessGLogWareMessage(e.PlcMessage);
        }
        else
        {
            _driverState = e.NotificationType;
            switch (_driverState)
            {
                case DriverNotificationType.Online:
                    logger.LogInformation("GLogWare is now CONNECTED !");
                    break;
                case DriverNotificationType.Offline:
                    logger.LogInformation("GLogWare is now DISCONNECTED !");
                    break;
                case DriverNotificationType.TelegramSent:
                    logger.LogInformation("GLogWare has a telegram to send");
                    break;
                case DriverNotificationType.TelegramSentAcknowledged:
                    logger.LogInformation("GLogWare acknowledged the sent telegram");
                    break;
            }
        }

        logger.LeaveMethod();
    }

    private async Task ProcessGLogWareMessage(PlcMessage pm)
    {
        logger.EnterMethod();

        try
        {
            switch (pm.Identifier)
            {
                case PlcMessageIdentifiers.TARG:
                    TARG targ = (TARG)pm.Data!;
                    await Process_TARG(targ);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing GLogWareMessage");
        }

        logger.LeaveMethod();
    }
}