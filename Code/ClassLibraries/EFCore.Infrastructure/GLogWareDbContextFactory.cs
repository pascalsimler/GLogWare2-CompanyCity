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

        DatabaseProviderHelper.SetDatabaseProvider();
        string connectionString = configuration[$"Database:ConnectionString"]!;
        Console.WriteLine($"Database:ConnectionString=[{connectionString}]");


        var options =
#if SQLSERVER
            new DbContextOptionsBuilder<GLogWareDbContext>()
                .UseSqlServer(connectionString)
                .Options;
#endif
#if ORACLE
            new DbContextOptionsBuilder<GLogWareDbContext>()
                .UseOracle(
                    connectionString, 
                    x => x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)
                )
                .Options;
#endif
#if POSTGRES
            new DbContextOptionsBuilder<GLogWareDbContext>()
                .UseNpgsql(connectionString)
                .Options;
#endif
#if MYSQL
            new DbContextOptionsBuilder<GLogWareDbContext>()
                .UseMySQL(connectionString)
                .Options;
#endif

        return new GLogWareDbContext(options);
    }
}
