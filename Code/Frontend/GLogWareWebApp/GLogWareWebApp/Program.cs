using GLogWareWebApp.Components;
using Gudel.GLogWare.Configuration;
using Gudel.GLogWare.EFCore;
using Gudel.GLogWare.Infrastructure;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.UI.Infrastructure;
using Serilog;
using Serilog.Events;

string configKey = "projectRootPath";
string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"{configKey}=[{projectRootPath}]");

var builder = WebApplication.CreateBuilder(args);
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
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
    ;
}

var serilogLogger = loggerConfig
    .WriteTo.Console(outputTemplate: logMessageTemplate)
    .WriteTo.File(
        path: ConfigurationHelper.GetLogFilePath(projectRootPath, "GLogWareWebApp"),
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        rollingInterval: RollingInterval.Day,
        outputTemplate: logMessageTemplate)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(serilogLogger);

using var startupLoggerFactory = LoggerFactory.Create(lb => lb.AddSerilog(serilogLogger));
var logger = startupLoggerFactory.CreateLogger("Startup");

logger.LogKeyValue("projectRootPath", projectRootPath);
configKey = "Database:GLogWareBusiness:ConnectionString";
string connectionString = builder.Configuration[configKey]!;
logger.LogKeyValue(configKey, connectionString);
configKey = "Database:GLogWareUI:ConnectionString";
string UIConnectionString = builder.Configuration[configKey]!;
logger.LogKeyValue(configKey, UIConnectionString);
configKey = "Project:Trigram";
string trigram = builder.Configuration[configKey]!;
logger.LogKeyValue(configKey, trigram);


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbProviderContext<GLogWareUIDbContext>(UIConnectionString);
builder.Services.AddDbProviderContext<GLogWareDbContext>(connectionString);
 
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = $"{trigram}-GLogWareWebApp";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(GLogWareWebApp.Client._Imports).Assembly);

app.Run();
