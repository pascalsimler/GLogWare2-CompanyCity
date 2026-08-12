using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Gudel.GLogWare.EFCore.Infrastructure;

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

    public static IServiceCollection AddGLogWareDbContext(
        this IServiceCollection services, string connectionString)
    {
        SetDatabaseProvider();

#if SQLSERVER
        services.AddDbContext<GLogWareDbContext>(
            options => options.UseSqlServer(
                connectionString,
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif
#if ORACLE
        services.AddDbContext<GLogWareDbContext>(
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
        services.AddDbContext<GLogWareDbContext>(
            options => options.UseNpgsql(
                connectionString, 
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif
#if MYSQL
        services.AddDbContext<GLogWareDbContext>(
            options => options.UseMySQL(
                connectionString, 
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif

        return services;
    }

    public static IServiceCollection AddGLogWareDbContextFactory(
       this IServiceCollection services, string connectionString)
    {
        SetDatabaseProvider();

#if SQLSERVER
        services.AddDbContextFactory<GLogWareDbContext>(
            options => options.UseSqlServer(
                connectionString,
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif
#if ORACLE
        services.AddDbContextFactory<GLogWareDbContext>(
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
        services.AddDbContextFactory<GLogWareDbContext>(
            options => options.UseNpgsql(
                connectionString, 
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif
#if MYSQL
        services.AddDbContextFactory<GLogWareDbContext>(
            options => options.UseMySQL(
                connectionString, 
                x => x.MigrationsAssembly(typeof(DatabaseProvider).Assembly.FullName)
            )
        );
#endif

        return services;
    }

    public static GLogWareDbContext GetGLogWareDbContext(string connectionString)
    {
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