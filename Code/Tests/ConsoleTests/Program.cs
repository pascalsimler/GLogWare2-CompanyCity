using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

Console.WriteLine(DateTimeOffset.Now);
string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"projectRootPath=[{projectRootPath}]");
string databaseProvider = DatabaseProviderHelper.GetDatabaseProvider().ToString();
Console.WriteLine($"databaseProvider=[{databaseProvider}]");

var configuration = new ConfigurationBuilder()
      .SetBasePath(projectRootPath) // base path for relative files
      .AddJsonFile(
          Path.Combine(ConfigurationHelper.GetConfigPath(projectRootPath), "config.json"),
          optional: false,
          reloadOnChange: true)
      .Build();
string connectionString = configuration[$"Database:ConnectionString_{databaseProvider}"]!;
Console.WriteLine($"connectionString=[{connectionString}");

GLogWareDbContext db = DatabaseProviderHelper.GetGLogWareDbContext(connectionString);
Console.WriteLine("DbContext successfully created");

//foreach (var inv in db.VInventories)
//{
//    Console.WriteLine($"{inv.Place} - {inv.Amount}");
//}

while (true)
{
    Console.WriteLine("----------------------");
    foreach (var a in db.Places.Where(p => p.ModifiedBy == "CRUCHOT"))
    {
        Console.WriteLine(
            $"Name=[{a.Name}]" +
            $", CreatedAt=[{a.CreatedAt?.ToString("dd.MM.yyyy HH:mm:ss.fff")}]" +
            $", LastUpdateAt=[{a.ModifiedAt?.ToString("dd.MM.yyyy HH:mm:ss.fff")}]"
        );
    }
    Console.Write("Again (y/n) ? ");
    string choice = Console.ReadLine()!;
    if (choice.ToLower() != "y") break;
}

await db.Places
    .Where(x => x.XCell == "2")
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(x => x.ModifiedBy, x => "FOUGASSE")
        .SetProperty(x => x.ModifiedAt, x => DateTime.Now)
    );
await db.SaveChangesAsync();