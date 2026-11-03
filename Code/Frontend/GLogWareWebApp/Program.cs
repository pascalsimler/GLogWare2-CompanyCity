using GLogWareWebApp.Components;
using GLogWareWebApp.Services;
using Gudel.GLogWare.Configuration;
using Gudel.GLogWare.EFCore;
using Gudel.GLogWare.Infrastructure;
using Gudel.GLogWare.Interfaces;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.MQTTMessageBus;
using Gudel.GLogWare.Secutity;
using MudBlazor.Services;
using MudBlazor.Translations;
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
        //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
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
string connectionString = builder.Configuration[$"Database:GLogWare:ConnectionString"]!;
logger.Information($"connectionString=[{connectionString}]");
string trigram = builder.Configuration[$"Project:Trigram"]!;
logger.Information($"trigram=[{trigram}]");


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddMudTranslations();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDbProviderContext<GLogWareDbContext>(connectionString);
builder.Services.AddSingleton<IMessageBus, MQTTMessageBus>();
//builder.Services.AddSingleton<MessageBusService>();
//builder.Services.AddHostedService<HostedMessageBusService>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<ClusterServiceManager>();
builder.Services.AddSingleton<GLogWareWebApp.Services.ServiceLogReader>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = $"{trigram}-GLogWareWebApp";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapGet("/administration/cluster-services/logs/{name}/download", (string name, GLogWareWebApp.Services.ServiceLogReader reader) =>
{
    if (!reader.IsValid(name))
    {
        return Results.BadRequest("Invalid service name");
    }

    try
    {
        return Results.File(reader.OpenRead(name), "text/plain", $"{name}.{DateTime.Now:yyyy-MM-dd}.log");
    }
    catch (IOException)
    {
        return Results.NotFound("Log file not found");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Problem("Access to the log file is denied", statusCode: 403);
    }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
