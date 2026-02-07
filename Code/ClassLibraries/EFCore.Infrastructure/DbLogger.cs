using Gudel.GLogWare.EFCore.Domain;
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
        string message,
        CancellationToken ct = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            db.Protocols.Add(new Protocol
            {
                Message = message
            });

            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // ⚠️ NEVER throw from logging
            // swallow intentionally
        }
    }
}
