using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.Extensions.Configuration;

JobTypeIdentifiers jt = JobTypeIdentifiers.RELOCATION;
Console.WriteLine(jt.ToString());
return;

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
string connectionString = configuration[$"ConnectionString_{databaseProvider}"]!;
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
    foreach (var a in db.Areas)
    {
        Console.WriteLine(
            $"Name=[{a.Name}]" +
            $", CreatedAt=[{a.CreatedAt?.ToString("dd.MM.yyyy HH:mm:ss.fff")}]" +
            $", LastUpdateAt=[{a.LastUpdatedAt?.ToString("dd.MM.yyyy HH:mm:ss.fff")}]"
        );
    }
    Console.Write("Again (y/n) ? ");
    string choice = Console.ReadLine()!;
    if (choice.ToLower() != "y") break;
}

//Place pl = new Place();
//pl.Name = "KOM-1-1";
//pl.AreaName = "KOM";
//db.Places.Add(pl);

var komArea = db.Areas.Where(a => a.Name == "GANTRY").FirstOrDefault();
if (komArea != null)
{
    komArea.Comments = DateTime.Now.ToString("HH:mm:ss");
    komArea.LastUpdatedBy = "Cruchot";
}

db.SaveChanges();