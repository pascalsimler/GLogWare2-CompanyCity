using GLogWareWebApp.Client.Pages;
using GLogWareWebApp.Components;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;
using Serilog;
using Serilog.Events;

string projectRootPath = ConfigurationHelper.GetProjectRootPath();
Console.WriteLine($"projectRootPath=[{projectRootPath}]");

var builder = WebApplication.CreateBuilder(args);
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
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
    ;
}

var logger = loggerConfig
    .WriteTo.Console(outputTemplate: logMessageTemplate)
    .WriteTo.File(
        path: ConfigurationHelper.GetLogFilePath(projectRootPath, "GLogWareWebApp"),
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

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

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
