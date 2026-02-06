using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public static class DatabaseProvider
{
    public const string Postgres = "Postgres";
    public const string SqlServer = "SqlServer";
    public const string Oracle = "Oracle";
    public const string MySql = "MySql";
    public const string Unknown = "Unknown";

    public static string GetDatabaseProviderName()
    {
        string providerName = Unknown;

#if USE_POSTGRES
        providerName = Postgres;
#endif
#if USE_SQLSERVER
        providerName = SqlServer;
#endif
#if USE_ORACLE
        providerName = Oracle;
#endif
#if USE_MYSQL
        providerName = MySql;
#endif

        return providerName;
    }

    public static string GetNowSql()
    {
        return GetDatabaseProviderName() switch
        {
            DatabaseProvider.Oracle => "SYSTIMESTAMP",
            DatabaseProvider.SqlServer => "SYSDATETIMEOFFSET()",
            DatabaseProvider.Postgres => "CURRENT_TIMESTAMP",
            DatabaseProvider.MySql => "NOW()",
            _ => string.Empty
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

        return GetDatabaseProviderName() switch
        {
            DatabaseProvider.Oracle => snake.ToUpperInvariant(),
            DatabaseProvider.SqlServer => name,
            DatabaseProvider.Postgres => snake.ToLowerInvariant(),
            DatabaseProvider.MySql => name,
            _ => name
        };
    }

    public static IServiceCollection AddGLogWareDbContext(
        this IServiceCollection services,
        string connectionString)
    {

#if USE_POSTGRES
        //services.AddDbContext<GLogWareDbContext>(options =>
        //    options.UseNpgsql(
        //        connectionString,
        //        x => x.MigrationsAssembly(
        //            typeof(DatabaseProvider).Assembly.FullName)));

        services.AddDbContextFactory<GLogWareDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                x => x.MigrationsAssembly(
                    typeof(DatabaseProvider).Assembly.FullName)));
#endif

#if USE_SQLSERVER
        //services.AddDbContext<GLogWareDbContext>(options =>
        //    options.UseSqlServer(
        //        connectionString,
        //        x => x.MigrationsAssembly(
        //            typeof(DatabaseProvider).Assembly.FullName)));

        services.AddDbContextFactory<GLogWareDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                x => x.MigrationsAssembly(
                    typeof(DatabaseProvider).Assembly.FullName)));
#endif

        return services;
    }
}
