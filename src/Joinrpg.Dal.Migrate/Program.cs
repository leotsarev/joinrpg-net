using JoinRpg.Dal.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Joinrpg.Dal.Migrate;

internal class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IJoinDbContextConfiguration>(
                    new DbContextConfiguration
                    {
                        ConnectionString = hostContext.Configuration.GetConnectionString(DbConsts.DefaultConnection),
                        DetailedErrors = true,
                        SensitiveLogging = true,
                    });
                services.AddDbContext<MyDbContext>();
                services.AddHostedService<MigrateHostService>();
            });
}
