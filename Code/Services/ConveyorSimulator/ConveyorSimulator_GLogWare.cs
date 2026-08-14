using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using Gudel.GLogWare.PlcDriver;

namespace Gudel.GLogWare.Services.ConveyorSimulator;

public partial class ConveyorSimulator
{
    #region Private members
    private DriverNotificationType _driverState;
    #endregion region

    private void LoadGLogWareConfiguration()
    {
        _logger.EnterMethod();

        _plcSimulatorDriver.LoadConfiguration(_configPath);

        _logger.LeaveMethod();
    }

    private async Task StartPlcSimulatorDriverAsync(CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        _plcSimulatorDriver.DriverNotification += OnPlcSimulatorDriverNotification;
        await _plcSimulatorDriver.StartAsync(cancellationToken);

        _logger.LeaveMethod();
    }

    private async void OnPlcSimulatorDriverNotification(object? sender, DriverNotificationEventArgs e)
    {
        _logger.EnterMethod();

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
                    _logger.LogInformation("GLogWare is now CONNECTED !");
                    break;
                case DriverNotificationType.Offline:
                    _logger.LogInformation("GLogWare is now DISCONNECTED !");
                    break;
                case DriverNotificationType.TelegramSent:
                    _logger.LogInformation("GLogWare has a telegram to send");
                    break;
                case DriverNotificationType.TelegramSentAcknowledged:
                    _logger.LogInformation("GLogWare acknowledged the sent telegram");
                    break;
            }
        }

        _logger.LeaveMethod();
    }

    private async Task ProcessGLogWareMessage(PlcMessage pm)
    {
        _logger.EnterMethod();

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
            _logger.LogError(ex, "Error processing GLogWareMessage");
        }

        _logger.LeaveMethod();
    }
}