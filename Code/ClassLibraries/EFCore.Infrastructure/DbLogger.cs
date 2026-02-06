using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class DbLogger
{
    private readonly IDbContextFactory<GLogWareDbContext> _dbFactory;

    public DbLogger(IDbContextFactory<GLogWareDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task WriteAsync(
        string level,
        string message,
        string? context = null,
        CancellationToken ct = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            //db.AppLogs.Add(new AppLogEntry
            //{
            //    Level = level,
            //    Message = message,
            //    Context = context
            //});

            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // ⚠️ NEVER throw from logging
            // swallow intentionally
        }
    }
}
