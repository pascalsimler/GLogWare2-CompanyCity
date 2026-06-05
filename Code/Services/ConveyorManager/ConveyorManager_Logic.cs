using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.Services.ConveyorManager;

public partial class ConveyorManager
{
    #region Private members

    #endregion

    private async Task Process_STAT(STATConveyor stat)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        string json = GLogWareMessage.Serialize<STATConveyor>(stat);
        _logger.LogInformation($"stat=[\r\n{json}\r\n]");

      
        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task Process_ARIV(ARIV ariv)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        string json = GLogWareMessage.Serialize<ARIV>(ariv);
        _logger.LogInformation($"ariv=[\r\n{json}\r\n]");

     
        _logger.LogInformation(LogMessages.LeaveMethod);
    }
}