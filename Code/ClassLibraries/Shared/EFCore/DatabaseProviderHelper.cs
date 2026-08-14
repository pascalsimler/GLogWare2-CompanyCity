using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Gudel.GLogWare.EFCore;

public static class DatabaseProviderHelper
{
    public static DatabaseProvider databaseProvider = DatabaseProvider.Unknown;

    public static void SetDatabaseProvider()
    {
        databaseProvider = DatabaseProvider.Unknown;

#if SQLSERVER
        databaseProvider = DatabaseProvider.SqlServer;
#endif
#if ORACLE
        databaseProvider = DatabaseProvider.Oracle;
#endif
#if POSTGRES
        databaseProvider = DatabaseProvider.Postgres;
#endif
#if MYSQL
        databaseProvider = DatabaseProvider.MySql;
#endif

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

    public static IServiceCollection AddDbProviderContext<TDbContext>(
        this IServiceCollection services, string connectionString
    ) where TDbContext : DbContext
    {
        SetDatabaseProvider();

#if SQLSERVER
        services.AddDbContext<TDbContext>(
            options => options.UseSqlServer(
                connectionString,
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif
#if ORACLE
        services.AddDbContext<TDbContext>(
            options => options.UseOracle(
                connectionString, 
                x => {
                    x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName);
                    x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                }
            )
        );
#endif
#if POSTGRES
        services.AddDbContext<TDbContext>(
            options => options.UseNpgsql(
                connectionString, 
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif
#if MYSQL
        services.AddDbContext<TDbContext>(
            options => options.UseMySQL(
                connectionString, 
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif

        return services;
    }

    public static IServiceCollection AddDbProviderContextFactory<TDbContext>(
        this IServiceCollection services, string connectionString
    ) where TDbContext : DbContext
    {
        SetDatabaseProvider();

#if SQLSERVER
        services.AddDbContextFactory<TDbContext>(
            options => options.UseSqlServer(
                connectionString,
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif
#if ORACLE
        services.AddDbContextFactory<TDbContext>(
            options => options.UseOracle(
                connectionString, 
                x => {
                    x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName);
                    x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                }
            )
        );
#endif
#if POSTGRES
        services.AddDbContextFactory<TDbContext>(
            options => options.UseNpgsql(
                connectionString, 
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif
#if MYSQL
        services.AddDbContextFactory<TDbContext>(
            options => options.UseMySQL(
                connectionString, 
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif

        return services;
    }

    public static TDbContext GetDbProviderContext<TDbContext>(string connectionString) where TDbContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();

#if SQLSERVER
        optionsBuilder.UseSqlServer(connectionString);
#endif
#if ORACLE
        optionsBuilder.UseOracle(
            connectionString, 
            x => x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)
        );
#endif
#if POSTGRES
        optionsBuilder.UseNpgsql(connectionString);
#endif
#if MYSQL
        optionsBuilder.UseMySQL(connectionString
#endif

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
    }
}