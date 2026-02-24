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

    public async Task WriteProtocolAsync(Protocol protocol)
    {
        try
        {
            _db.Protocols.Add(protocol);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // ⚠️ NEVER throw from logging
            // swallow intentionally
        }
    }

    public async Task WriteLogPlcAsync(LogPlc logPlc)
    {
        try
        {
            _db.LogPlcs.Add(logPlc);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // ⚠️ NEVER throw from logging
            // swallow intentionally
        }
    }

    public async Task WriteLogErpAsync(LogErp logErp)
    {
        try
        {
            _db.LogErps.Add(logErp);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // ⚠️ NEVER throw from logging
            // swallow intentionally
        }
    }
}
