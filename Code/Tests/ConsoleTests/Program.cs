using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.Extensions.Configuration;


Console.WriteLine(DateTimeOffset.Now);
string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"projectRootPath=[{projectRootPath}]");
Console.WriteLine($"databaseProviderName=[{DatabaseProviderHelper.GetDatabaseProviderName()}]");

var configuration = new ConfigurationBuilder()
      .SetBasePath(projectRootPath) // base path for relative files
      .AddJsonFile(
          Path.Combine(ConfigurationHelper.GetConfigPath(projectRootPath), "config.json"),
          optional: false,
          reloadOnChange: true)
      .Build();
string connectionString = configuration["ConnectionString"]!;
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
            $", CreatedAt(localtime)=[{a.CreatedAt?.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss.fff")}]" +
            $", CreatedAt(UTC)=[{a.CreatedAt?.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss.fff")}]" +
            $", LastUpdateAt(localtime)=[{a.LastUpdatedAt?.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss.fff")}]" +
            $", LastUpdateAt(UTC)=[{a.LastUpdatedAt?.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss.fff")}]"
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