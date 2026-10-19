using Gudel.GLogWare.Configuration;
using Gudel.GLogWare.EFCore;
using Gudel.GLogWare.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"projectRootPath=[{projectRootPath}]");

var configuration = new ConfigurationBuilder()
      .SetBasePath(projectRootPath) // base path for relative files
      .AddJsonFile(
          Path.Combine(ConfigurationHelper.GetConfigPath(projectRootPath), "config.json"),
          optional: false,
          reloadOnChange: true)
      .Build();

string connectionString = configuration[$"Database:GLogWare:ConnectionString"]!;
Console.WriteLine($"connectionString=[{connectionString}");

GLogWareDbContext db = DatabaseProviderHelper.GetDbProviderContext<GLogWareDbContext>(connectionString);
Console.WriteLine("DbContext successfully created");

//foreach (var inv in db.ViewInventories)
//{
//    Console.WriteLine($"{inv.Name} - {inv.TireCount}");
//}
//Console.Write("Press any key to continue ...");
//Console.ReadLine();

IQueryable<Zob> b;
b = db.Statistics
    .Where(s => s.Category == "Bridge")
    .GroupBy(s=>new { s.Day.Year, s.Day.Month })
    .Select(s => new Zob
    {
        Year = s.Key.Year,
        Month = s.Key.Month,
        c = s.Sum(x => x.Counter02)
    });
Console.WriteLine(b.ToQueryString());
Console.ReadLine();
foreach (var f in b)
{
    Console.WriteLine($"{f.Year}-{f.Month} - {f.c}");
}
Console.Write("Press any key to continue ...");
Console.ReadLine();

//var q = db.ViewStatisticFillLevels
//    .Where(f => f.Area == "KOM-1")
//    .OrderBy(f => f.Day)
//    .ThenBy(f => f.Hour)
//    ;
//foreach (var f in q)
//{
//    Console.WriteLine($"{f.Area} - {f.Day} - {f.Hour} - {f.CountOccupied} ({f.PercentOccupied} %)");
//}
//Console.Write("Press any key to continue ...");
//Console.ReadLine();

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


public class Zob
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int c { get; set; }
}