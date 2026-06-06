using Gudel.GLogWare.LegacyPlcDriver;
using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.Services.ConveyorSimulator;

public partial class ConveyorSimulator
{
    #region Private members
    private DriverNotificationType _driverState;
    #endregion region

    private void LoadGLogWareConfiguration()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _plcSimulatorDriver.LoadConfiguration($"Conveyors:{OP}");
    }

    private async Task StartPlcSimulatorDriverAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _plcSimulatorDriver.DriverNotification += OnPlcSimulatorDriverNotification;
        await _plcSimulatorDriver.StartAsync(cancellationToken);
    }

    private async void OnPlcSimulatorDriverNotification(object? sender, DriverNotificationEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        if (e.notificationType == DriverNotificationType.TelegramReceived)
        {
            await ProcessGLogWareMessage(e.plcMessage);
        }
        else
        {
            _driverState = e.notificationType;
            switch (_driverState)
            {
                case DriverNotificationType.Online:
                    _logger.LogInformation($"GLogWare is now CONNECTED !");
                    break;
                case DriverNotificationType.Offline:
                    _logger.LogInformation($"GLogWare is now DISCONNECTED !");
                    break;
                case DriverNotificationType.TelegramSent:
                    _logger.LogInformation($"GLogWare has a telegram to send");
                    break;
                case DriverNotificationType.TelegramSentAcknowledged:
                    _logger.LogInformation($"GLogWare acknowledged the sent telegram");
                    break;
            }
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task ProcessGLogWareMessage(PlcMessage pm)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

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
    }
}