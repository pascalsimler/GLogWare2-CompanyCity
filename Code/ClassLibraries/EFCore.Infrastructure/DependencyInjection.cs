using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGLogWareDbContext(
        this IServiceCollection services,
        string connectionString)
    {

#if USE_POSTGRES
        //services.AddDbContext<GLogWareDbContext>(options =>
        //    options.UseNpgsql(
        //        connectionString,
        //        x => x.MigrationsAssembly(
        //            typeof(DependencyInjection).Assembly.FullName)));

        services.AddDbContextFactory<GLogWareDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                x => x.MigrationsAssembly(
                    typeof(DependencyInjection).Assembly.FullName)));
#endif

#if USE_SQLSERVER
        //services.AddDbContext<GLogWareDbContext>(options =>
        //    options.UseSqlServer(
        //        connectionString,
        //        x => x.MigrationsAssembly(
        //            typeof(DependencyInjection).Assembly.FullName)));

        services.AddDbContextFactory<GLogWareDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                x => x.MigrationsAssembly(
                    typeof(DependencyInjection).Assembly.FullName)));
#endif

        return services;
    }
}
