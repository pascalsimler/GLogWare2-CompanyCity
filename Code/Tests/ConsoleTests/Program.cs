using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine(DateTimeOffset.Now);
string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"projectRootPath=[{projectRootPath}]");
Console.WriteLine($"databaseproviderName=[{DatabaseProvider.GetDatabaseProviderName()}]");

var configuration = new ConfigurationBuilder()
      .SetBasePath(projectRootPath) // base path for relative files
      .AddJsonFile(
          Path.Combine(ConfigurationHelper.GetConfigPath(projectRootPath), "config.json"),
          optional: false,
          reloadOnChange: true)
      .Build();
string connectionString = configuration["ConnectionString"];
Console.WriteLine($"connectionString=[{connectionString}");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddGLogWareDbContext(connectionString);
    })
    .Build();


// Resolve and use DbContext
using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<GLogWareDbContext>();

Console.WriteLine("DbContext successfully created");

//foreach (var inv in db.VInventories)
//{
//    Console.WriteLine($"{inv.Place} - {inv.Amount}");
//}

Console.WriteLine("----------------------");

foreach (var a in db.Areas)
{
    Console.WriteLine($"Name=[{a.Name}], CreatedAt=[{a.CreatedAt?.ToString("dd.MM.yyyy HH:mm:ss.fff")}], LastUpdateAt=[{a.LastUpdatedAt?.ToString("dd.MM.yyyy HH:mm:ss.fff")}]");
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