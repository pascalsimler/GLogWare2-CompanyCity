using Gudel.GLogWare.Configuration;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.LegacyPlcDriver;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.MessageBus;
using Gudel.GLogWare.MQTTMessageBus;
using Gudel.GLogWare.PlcDriver;
using Gudel.GLogWare.Services.ConveyorSimulator;
using Serilog;

ConveyorSimulator.OP = Environment.GetEnvironmentVariable("OP");
if (ConveyorSimulator.OP == null)
{
    Console.WriteLine("OP environement variable is not set !!! ==> Hasta la vista ...");
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
    .Enrich.With(new CustomLoggerEnricher());

if (enableSystemLogging == 0)
{
    loggerConfig
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning);
}

var logger = loggerConfig
    .WriteTo.Console(outputTemplate: logMessageTemplate)
    .WriteTo.File(
        path: ConfigurationHelper.GetLogFilePath(projectRootPath, ConveyorSimulator.ServiceName),
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        rollingInterval: RollingInterval.Day,
        outputTemplate: logMessageTemplate)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

logger.Information($"ServiceName=[{ConveyorSimulator.ServiceName}]");
logger.Information($"projectRootPath=[{projectRootPath}]");
string providerName = builder.Configuration[$"Database:Provider"]!;
logger.Information($"providerName=[{providerName}]");
DatabaseProviderHelper.SetDatabaseProvider(providerName);
string connectionString = builder.Configuration[$"Database:ConnectionString"]!;
logger.Information($"connectionString=[{connectionString}]");
string trigram = builder.Configuration[$"Project:Trigram"]!;
logger.Information($"trigram=[{trigram}]");

builder.Services.AddGLogWareDbContextFactory(connectionString);
builder.Services.AddSingleton<IMessageBus, MQTTMessageBus>();
builder.Services.AddSingleton<IPlcDriver, LegacyPlcSimulatorDriver>();
builder.Services.AddHostedService<ConveyorSimulator>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = $"{trigram}-BridgeSimulator-{ConveyorSimulator.OP}";
});

var host = builder.Build();
host.Run();
