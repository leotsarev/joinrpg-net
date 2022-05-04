using JoinRpg.Dal.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Joinrpg.Dal.Migrate;

internal class MigrateHostService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _services;

    public MigrateHostService(
        IServiceProvider services,
        ILogger<MigrateHostService> logger)
    {
        _logger = logger;
        _services = services;
    }

    private async Task MigrateAsync(DbContext dbContext, CancellationToken stoppingToken)
    {
        var lastAppliedMigration = (await dbContext.Database.GetAppliedMigrationsAsync(stoppingToken)).LastOrDefault();
        if (!string.IsNullOrEmpty(lastAppliedMigration))
        {
            _logger.LogInformation("Last applied migration: {LastAppliedMigration}", lastAppliedMigration);
        }

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        var pendingMigrations = string.Join("; ", await dbContext.Database.GetPendingMigrationsAsync(stoppingToken));
        foreach (var pm in pendingMigrations)
        {
            _logger.LogInformation("Pending migration: {PendingMigration}", pm);
        }

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        _logger.LogInformation("Applying migrations...");
        await dbContext.Database.MigrateAsync(stoppingToken);
        _logger.LogInformation("Database has been successfully migrated");
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting migrator...");
        await using var scope = _services.CreateAsyncScope();
        try
        {
            await MigrateAsync(
                scope.ServiceProvider.GetRequiredService<MyDbContext>(),
                stoppingToken);

            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Terminating by cancellation token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing migrator");
        }
    }
}
