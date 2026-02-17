using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace JoinRpg.Common.WebInfrastructure;

public static class DbContextRegisterHelper
{
    public static bool AddJoinEfCoreDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string connectionStringName,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        _ = services.AddDbContext<TContext>(
        options =>
        {
            _ = options
                .UseNpgsql(connectionString)
                .EnableSensitiveDataLogging(environment.IsDevelopment())
                .EnableDetailedErrors(environment.IsDevelopment())
                .UseExceptionProcessor()
                .ConfigureWarnings(
                    b => b.Log(
                        (RelationalEventId.CommandExecuted, LogLevel.Debug)));
            optionsAction?.Invoke(options);
        })

            .AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: $"{connectionStringName}-db",
                failureStatus: HealthStatus.Degraded);

        return true;
    }
}
