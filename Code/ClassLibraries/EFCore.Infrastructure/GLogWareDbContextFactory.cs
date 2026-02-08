using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class GLogWareDbContextFactory : IDesignTimeDbContextFactory<GLogWareDbContext>
{
    public GLogWareDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("Default");

#if USE_POSTGRES
        var options = new DbContextOptionsBuilder<GLogWareDbContext>()
            .UseNpgsql(connectionString)
            .Options;
#endif
#if USE_SQLSERVER
        var options = new DbContextOptionsBuilder<GLogWareDbContext>()
            .UseSqlServer(connectionString)
            .Options;
#endif

        return new GLogWareDbContext(options);
    }
}
