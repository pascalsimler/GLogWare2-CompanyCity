using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class DbLogger
{
    private readonly GLogWareDbContext _db;

    public DbLogger(IDbContextFactory<GLogWareDbContext> factory)
    {
        _db = factory.CreateDbContext();
    }

    public async Task WriteAsync(
        string message,
        CancellationToken ct = default)
    {
        try
        {
            //await using var db = await _dbFactory.CreateDbContextAsync(ct);

            _db.Protocols.Add(new Protocol
            {
                Message = message
            });

            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // ⚠️ NEVER throw from logging
            // swallow intentionally
        }
    }
}
