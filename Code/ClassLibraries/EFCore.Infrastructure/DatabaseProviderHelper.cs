using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public static class DatabaseProviderHelper
{
    public static DatabaseProvider databaseProvider = DatabaseProvider.Unknown;

    public static void SetDatabaseProvider(string providerName)
    {
        databaseProvider = providerName switch
        {
            "SqlServer" => DatabaseProvider.SqlServer,
            "Oracle" => DatabaseProvider.Oracle,
            "Postgres" => DatabaseProvider.Postgres,
            "MySql" => DatabaseProvider.MySql,
            _ => DatabaseProvider.Unknown
        };
    }

    public static string GetNowSql()
    {
        return databaseProvider switch 
        {
            DatabaseProvider.Oracle => "LOCALTIMESTAMP",
            DatabaseProvider.Postgres => "LOCALTIMESTAMP",
            DatabaseProvider.MySql => "CURRENT_TIMESTAMP(6)",
            _ => "GETDATE()"
        };
    }

    public static string GetBlobType()
    {
        return databaseProvider switch
        {
            DatabaseProvider.Oracle => "CLOB",
            DatabaseProvider.Postgres => "TEXT",
            DatabaseProvider.MySql => "LONGTEXT",
            _ => "NVARCHAR(MAX)"
        };
    }

    public static string ToProviderName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // PascalCase → snake_case
        var snake = Regex.Replace(
            name,
            "([a-z0-9])([A-Z])",
            "$1_$2",
            RegexOptions.Compiled);

        return databaseProvider switch
        {
            DatabaseProvider.Oracle => snake.ToUpperInvariant(),
            DatabaseProvider.Postgres => snake.ToLowerInvariant(),
            _ => name
        };
    }

    public static IServiceCollection AddGLogWareDbContext(
        this IServiceCollection services, string connectionString)
    {
        switch (databaseProvider)
        {
            case DatabaseProvider.Oracle:
                services.AddDbContext<GLogWareDbContext>(
                    options => options.UseOracle(
                        connectionString, 
                        x => {
                            x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName);
                            x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                        }
                    )
                );
                break;
            case DatabaseProvider.Postgres:
                services.AddDbContext<GLogWareDbContext>(
                    options => options.UseNpgsql(
                        connectionString, 
                        x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
                    )
                );
                break;
            case DatabaseProvider.MySql:
                services.AddDbContext<GLogWareDbContext>(
                    options => options.UseMySQL(
                        connectionString, 
                        x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
                    )
                );
                break;
            default:
                services.AddDbContext<GLogWareDbContext>(
                    options => options.UseSqlServer(
                        connectionString,
                        x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
                    )
                );
                break;
        }

        return services;
    }

    public static IServiceCollection AddGLogWareDbContextFactory(
       this IServiceCollection services, string connectionString)
    {
        switch (databaseProvider)
        {
            case DatabaseProvider.Oracle:
                services.AddDbContextFactory<GLogWareDbContext>(
                    options => options.UseOracle(
                        connectionString, 
                        x => {
                            x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName);
                            x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                        }
                    )
                );
                break;
            case DatabaseProvider.Postgres:
                services.AddDbContextFactory<GLogWareDbContext>(
                    options => options.UseNpgsql(
                        connectionString, 
                        x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
                    )
                );
                break;
            case DatabaseProvider.MySql:
                services.AddDbContextFactory<GLogWareDbContext>(
                    options => options.UseMySQL(
                        connectionString, 
                        x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
                    )
                );
                break;
            default:
                services.AddDbContextFactory<GLogWareDbContext>(
                    options => options.UseSqlServer(
                        connectionString,
                        x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
                    )
                );
                break;
        }

        return services;
    }

    public static GLogWareDbContext GetGLogWareDbContext(string connectionString)
    {
        var options = databaseProvider switch
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