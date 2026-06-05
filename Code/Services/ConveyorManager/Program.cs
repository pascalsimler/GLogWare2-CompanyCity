using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.LegacyPlcDriver;
using Gudel.GLogWare.Services.ConveyorManager;
using Gudel.GLogWare.Shared;
using Serilog;

ConveyorManager.OP = Environment.GetEnvironmentVariable("OP");
if (ConveyorManager.OP == null)
{
    Console.WriteLine("OP environement variable is not set !!! ==> Asta la vista ...");
    return;
}

string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"projectRootPath=[{projectRootPath}]");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile(
    Path.Combine(ConfigurationHelper.GetConfigPath(projectRootPath), "config.json"),
    optional: false,
    reloadOnChange: true);

string logMessageTemplate = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{ClassMethod}] {Message:lj}{NewLine}{Exception}";
int enableSystemLogging = builder.Configuration.GetValue<int>("EnableSystemLogging", 0);
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.With(new CustomLoggerEnricher());

if (enableSystemLogging == 0)
{
    loggerConfig
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    ;
}

var logger = loggerConfig
    .WriteTo.Console(outputTemplate: logMessageTemplate)
    .WriteTo.File(
        path: ConfigurationHelper.GetLogFilePath(projectRootPath, ConveyorManager.ServiceName!),
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        rollingInterval: RollingInterval.Day,
        outputTemplate: logMessageTemplate)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

logger.Information($"ConveyorManager.OP=[{ConveyorManager.OP}]");
logger.Information($"ConveyorManager.ServiceName=[{ConveyorManager.ServiceName}]");
logger.Information($"projectRootPath=[{projectRootPath}]");
string databaseProvider = DatabaseProviderHelper.GetDatabaseProvider().ToString();
logger.Information($"databaseProvider=[{databaseProvider}]");
string connectionString = builder.Configuration[$"Database:ConnectionString_{databaseProvider}"]!;
logger.Information($"connectionString=[{connectionString}]");
string trigram = builder.Configuration[$"Project:Trigram"]!;
logger.Information($"trigram=[{trigram}]");

builder.Services.AddGLogWareDbContextFactory(connectionString);
builder.Services.AddSingleton<IPlcDriver, LegacyPlcDriver>();
builder.Services.AddHostedService<ConveyorManager>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = $"{trigram}-BridgeManager-{ConveyorManager.OP}";
});

var host = builder.Build();
host.Run();
