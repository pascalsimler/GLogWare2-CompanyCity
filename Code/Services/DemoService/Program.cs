using Gudel.GLogWare.DemoService;
using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddGLogWareDbContext("Host=localhost;Port=5432;Database=GLogWare_CompanyCity;Username=admin;Password=*Gudel1954*");
   
var host = builder.Build();
host.Run();
