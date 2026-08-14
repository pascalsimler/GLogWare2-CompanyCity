using Gudel.GLogWare.Services.DemoService;
using Serilog;
using Gudel.GLogWare.Configuration;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.MessageBus;
using Gudel.GLogWare.MQTTMessageBus;
using Gudel.GLogWare.EFCore;
using Gudel.GLogWare.Infrastructure;

DemoService.ServiceName = "DemoService";

string configKey = "projectRootPath";
string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"{configKey}=[{projectRootPath}]");

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

var serilogLogger = loggerConfig
    .WriteTo.Console(outputTemplate: logMessageTemplate)
    .WriteTo.File(
        path: ConfigurationHelper.GetLogFilePath(projectRootPath, DemoService.ServiceName),
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        rollingInterval: RollingInterval.Day,
        outputTemplate: logMessageTemplate)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(serilogLogger);

using var startupLoggerFactory = LoggerFactory.Create(lb => lb.AddSerilog(serilogLogger));
var logger = startupLoggerFactory.CreateLogger("Startup");

logger.LogKeyValue("ServiceName", DemoService.ServiceName);
logger.LogKeyValue("projectRootPath", projectRootPath);
configKey = "Database:GLogWareBusiness:ConnectionString";
string connectionString = builder.Configuration[configKey]!;
logger.LogKeyValue(configKey, connectionString);
configKey = "Project:Trigram";
string trigram = builder.Configuration[configKey]!;
logger.LogKeyValue(configKey, trigram);

builder.Services.AddDbProviderContextFactory<GLogWareDbContext>(connectionString);
builder.Services.AddSingleton<IMessageBus, MQTTMessageBus>();
builder.Services.AddHostedService<DemoService>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = $"{trigram}-{DemoService.ServiceName}";
});

var host = builder.Build();
host.Run();