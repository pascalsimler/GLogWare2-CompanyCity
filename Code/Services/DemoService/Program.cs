using Gudel.GLogWare.DemoService;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Serilog;
using Serilog.Enrichers.CallerInfo;

Worker.ServiceName = "DemoService";
string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"projectRootPath=[{projectRootPath}]");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile(
    Path.Combine(ConfigurationHelper.GetConfigPath(projectRootPath), "config.json"),
    optional: false,
    reloadOnChange: true);

string logMessageTemplate = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{Method}] {Message:lj}{NewLine}{Exception}";
int enableEFCoreLogging = builder.Configuration.GetValue<int>("EnableEFCoreLogging", 0);
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithCallerInfo(false, new List<string> { "Gudel.GLogWare.DemoService" });

if (enableEFCoreLogging == 0)
{
    loggerConfig
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning);
}

var logger = loggerConfig
    .WriteTo.Console(outputTemplate: logMessageTemplate)
    .WriteTo.File(
        path: ConfigurationHelper.GetLogFilePath(projectRootPath, Worker.ServiceName),
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        rollingInterval: RollingInterval.Day,
        outputTemplate: logMessageTemplate)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

logger.Information($"projectRootPath=[{projectRootPath}]");
string databaseProvider = DatabaseProviderHelper.GetDatabaseProvider().ToString();
logger.Information($"databaseProvider=[{databaseProvider}]");
string connectionString = builder.Configuration[$"ConnectionString_{databaseProvider}"]!;
logger.Information($"connectionString=[{connectionString}]");


builder.Services.AddSingleton<DemoService>();
builder.Services.AddSingleton<DbLogger>();
builder.Services.AddGLogWareDbContextFactory(connectionString);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
