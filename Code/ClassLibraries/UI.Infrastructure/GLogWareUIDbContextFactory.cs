using Gudel.GLogWare.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.UI.Infrastructure;

public class GLogWareUIDbContextFactory : DbProviderContextFactory<GLogWareUIDbContext>
{
    protected override string ConfigPathConnectionString => $"Database:GLogWareUI:ConnectionString";

    protected override GLogWareUIDbContext CreateContext(
        DbContextOptions<GLogWareUIDbContext> options)
    {
        return new GLogWareUIDbContext(options);
    }
}
