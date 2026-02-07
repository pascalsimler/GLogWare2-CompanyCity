using Gudel.GLogWare.DemoService;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Microsoft.EntityFrameworkCore;
using Serilog;

Worker.ServiceName = "DemoService";
string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"projectRootPath=[{projectRootPath}]");

string logMessageTemplate = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: logMessageTemplate)
    .WriteTo.File(
        path: ConfigurationHelper.GetLogFilePath(projectRootPath, Worker.ServiceName),
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        rollingInterval: RollingInterval.Day,
        outputTemplate: logMessageTemplate)
    .CreateLogger();
logger.Information($"projectRootPath=[{projectRootPath}]");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile(
    Path.Combine(ConfigurationHelper.GetConfigPath(projectRootPath), "config.json"),
    optional: false,
    reloadOnChange: true);

string connectionString = builder.Configuration.GetValue<string>("ConnectionString")!;
logger.Information($"connectionString=[{connectionString}]");

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddHostedService<Worker>();
builder.Services.AddGLogWareDbContext(connectionString);
builder.Services.AddSingleton<DbLogger>();

var host = builder.Build();
host.Run();
