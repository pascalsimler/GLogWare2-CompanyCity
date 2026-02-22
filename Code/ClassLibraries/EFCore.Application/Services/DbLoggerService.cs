using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.EFCore.Application;

public class DbLoggerService
{
    private readonly GLogWareDbContext _db;

    public DbLoggerService(IDbContextFactory<GLogWareDbContext> factory)
    {
        _db = factory.CreateDbContext();
    }

    public async Task WriteAsync(string message)
    {
        try
        {
            //await using var db = await _dbFactory.CreateDbContextAsync(ct);

            _db.Protocols.Add(new Protocol
            {
                Message = message
            });

            await _db.SaveChangesAsync();
        }
        catch
        {
            // ⚠️ NEVER throw from logging
            // swallow intentionally
        }
    }
}
