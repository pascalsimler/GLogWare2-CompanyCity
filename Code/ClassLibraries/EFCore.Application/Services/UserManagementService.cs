using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.EFCore.Application;

public class UserManagementService
{
    private readonly GLogWareDbContext _db;

    public UserManagementService(IDbContextFactory<GLogWareDbContext> factory)
    {
        _db = factory.CreateDbContext();
    }
}
