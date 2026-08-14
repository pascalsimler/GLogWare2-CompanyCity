using Gudel.GLogWare.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Gudel.GLogWare.EFCore;

public abstract class DbProviderContextFactory<TDbContext> 
    : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    protected abstract string ConfigPathConnectionString { get; }
    public TDbContext CreateDbContext(string[] args)
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
        string connectionString = configuration[ConfigPathConnectionString]!;
        Console.WriteLine($"{ConfigPathConnectionString}=[{connectionString}]");

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();

#if SQLSERVER
        optionsBuilder.UseSqlServer(connectionString);
#endif
#if ORACLE
        optionBuilder.UseOracle(
            connectionString, 
            x => x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)
        );
#endif
#if POSTGRES
        optionBuilder.UseNpgsql(connectionString)
#endif
#if MYSQL
        optionBuilder.UseMySQL(connectionString)
#endif

        return CreateContext(optionsBuilder.Options);
    }

    protected abstract TDbContext CreateContext(DbContextOptions<TDbContext> options);
}
