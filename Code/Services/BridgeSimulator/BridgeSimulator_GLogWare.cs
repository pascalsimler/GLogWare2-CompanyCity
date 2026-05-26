using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.Services.BridgeSimulator;

public partial class BridgeSimulator
{
    #region Private members
    #endregion

    private void LoadGLogWareConfiguration()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _plcSimulatorDriver.LoadConfiguration($"GantryBridges:{OP}");
    }

    private async Task StartPlcSimulatorDriverAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _plcSimulatorDriver.DriverNotification += OnPlcDriverNotification;
        await _plcSimulatorDriver.StartAsync(cancellationToken);
    }

    private async void OnPlcDriverNotification(object? sender, DriverNotificationEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
    }

    //private async Task SendTelegram(PlcMessage pm)
    //{
    //    ProcessPlcMessage(pm);

    //    LegacyPlcTelegram t = new LegacyPlcTelegram();
    //    t.Identifier = pm.Identifier.ToString();
    //    t.Receiver = pm.Receiver;
    //    t.Sender = pm.Sender;
    //    switch (pm.Identifier)
    //    {
    //        case PlcMessageIdentifiers.STAT:
    //            STATBridge stat = GLogWareMessage.DeSerialize<STATBridge>(pm.Data!.ToString()!)!;
    //            STATBridgeStruct statStruct = STATBridgeStruct.FromMessage(stat);
    //            t.Data = statStruct.ToData();
    //            await SendToGLogWare(t, true);
    //            break;

    //        case PlcMessageIdentifiers.COMP:
    //            COMP comp = GLogWareMessage.DeSerialize<COMP>(pm.Data!.ToString()!)!;
    //            COMPStruct compStruct = COMPStruct.FromMessage(comp);
    //            t.Data = compStruct.ToData();
    //            await SendToGLogWare(t, true);
    //            break;
    //        default:
    //            break;
    //    }
    //    await _plcSimulatorDriver.SendToGLogWare(t, true);
    //}
   
}
