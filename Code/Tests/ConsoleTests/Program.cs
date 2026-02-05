using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddGLogWareDbContext("Host=localhost;Port=5432;Database=GLogWare_CompanyCity;Username=admin;Password=*Gudel1954*");
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
    Console.WriteLine(a.Name);
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