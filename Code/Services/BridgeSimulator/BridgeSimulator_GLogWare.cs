using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.Services.BridgeSimulator;

public partial class BridgeSimulator
{
    #region Private members
    private DriverNotificationType _driverState;
    #endregion region

    private void LoadGLogWareConfiguration()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _plcSimulatorDriver.LoadConfiguration($"GantryBridges:{OP}");
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
                case PlcMessageIdentifiers.ORDS:
                    ORDS ords = (ORDS)pm.Data!;
                    await Process_ORDS(ords);
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