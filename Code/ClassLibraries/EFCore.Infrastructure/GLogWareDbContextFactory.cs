using Gudel.GLogWare.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class GLogWareDbContextFactory : IDesignTimeDbContextFactory<GLogWareDbContext>
{
    public GLogWareDbContext CreateDbContext(string[] args)
    {
        string projectRootPath = ConfigurationHelper.GetProjectRootPath();
        Console.WriteLine($"projectRootPath=[{projectRootPath}]");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                Path.Combine(ConfigurationHelper.GetConfigPath(projectRootPath), "config.json"),
                optional: false,
                reloadOnChange: true
            )
            .Build();

        string providerName = configuration[$"Database:Provider"]!;
        Console.WriteLine($"Database:Provider=[{providerName}]");
        string connectionString = configuration[$"Database:ConnectionString"]!;
        Console.WriteLine($"Database:ConnectionString=[{connectionString}]");
        DatabaseProviderHelper.SetDatabaseProvider(providerName);

        var options = DatabaseProviderHelper.databaseProvider switch
        {
            DatabaseProvider.Oracle =>
                new DbContextOptionsBuilder<GLogWareDbContext>()
                    .UseOracle(
                        connectionString, 
                        x => x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)
                    )
                    .Options,
            DatabaseProvider.Postgres =>
                new DbContextOptionsBuilder<GLogWareDbContext>()
                    .UseNpgsql(connectionString)
                    .Options,
            DatabaseProvider.MySql =>
                new DbContextOptionsBuilder<GLogWareDbContext>()
                    .UseMySQL(connectionString)
                    .Options,
            _ =>
                new DbContextOptionsBuilder<GLogWareDbContext>()
                    .UseSqlServer(connectionString)
                    .Options,
        };

        return new GLogWareDbContext(options);
    }
}
