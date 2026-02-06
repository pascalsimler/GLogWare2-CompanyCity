using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MQTTnet.Extensions.ManagedClient;
using System;

namespace Gudel.GLogWare.DemoService;

public class Worker : IHostedService, IAsyncDisposable
{
    #region Private members
    private ILogger<Worker> _logger;
    private IConfiguration _configuration;
    private IDbContextFactory<GLogWareDbContext> _dbFactory;
    private GLogWareDbContext _dbCtx;
    private IManagedMqttClient _mqttClient;
    #endregion

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IDbContextFactory<GLogWareDbContext> dbFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _dbFactory = dbFactory;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting {Service} ...", nameof(Worker));

        _dbCtx = await _dbFactory.CreateDbContextAsync(stoppingToken);
        //_dbCtx = await _dbFactory.CreateDbContextAsync();

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping {Service} ...", nameof(Worker));

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }
}