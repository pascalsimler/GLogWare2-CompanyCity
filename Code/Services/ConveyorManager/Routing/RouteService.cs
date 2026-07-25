using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.Services.ConveyorManager;

public class RouteService
{
    #region Injected members
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<GLogWareDbContext> _dbContextFactory;
    private GLogWareDbContext _db = null!;
    #endregion

    #region private members
    private Dictionary<CachedRouteKey, CachedRoute> _routes = new();
    #endregion

    public RouteService(
        ILogger<ConveyorManager> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
    }

    private void CheckRoutesCache()
    {
    }
}
