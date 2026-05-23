using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager
{
    #region Private members
    #endregion region

    private void LoadPlcConfiguration()
    {
        _plcDriver.LoadConfiguration($"GantryBridges:{OP}");
    }

    private async Task StartPlcDriverAsync(CancellationToken cancellationToken)
    {
        _plcDriver.MessageReceived += OnPlcMessageReceived;
        _plcDriver.MessageAcknowledged += OnPlcMessageAcknowledged;
        await _plcDriver.StartAsync(cancellationToken);
    }

    private async void OnPlcMessageReceived(object? sender, PlcMessageReceivedEventArgs e)
    {
        await ProcessPlcMessage(e.plcMessage);  
    }

    private async void OnPlcMessageAcknowledged(object? sender, PlcMessageAcknowledgedEventArgs e)
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