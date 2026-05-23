using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gudel.GLogWare.LegacyPlcDriver;

public class LegacyPlcSimulatorDriver : IPlcDriver
{
    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<GLogWareDbContext> _dbContextFactory;
    #endregion

    #region Event handlers
    public event EventHandler<PlcMessageAcknowledgedEventArgs>? MessageAcknowledged;
    public event EventHandler<PlcMessageReceivedEventArgs>? MessageReceived;
    #endregion

    public LegacyPlcSimulatorDriver(
        ILogger<LegacyPlcDriver> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
    }

    public void LoadConfiguration(string op, string path)
    {
        throw new NotImplementedException();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SendAsync(PlcMessage plcMessage)
    {
        throw new NotImplementedException();
    }
}
