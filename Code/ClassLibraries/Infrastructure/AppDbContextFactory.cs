using Gudel.GLogWare.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.Infrastructure;

public class AppDbContextFactory : DbProviderContextFactory<GLogWareDbContext>
{
    protected override string ConfigPathConnectionString => $"Database:ConnectionString";

    protected override GLogWareDbContext CreateContext(
        DbContextOptions<GLogWareDbContext> options)
    {
        return new GLogWareDbContext(options);
    }
}
