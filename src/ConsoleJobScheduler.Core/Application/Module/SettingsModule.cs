using ConsoleJobScheduler.Core.Domain.Settings;
using ConsoleJobScheduler.Core.Domain.Settings.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Application.Module;

public sealed class SettingsModule
{
    private readonly IConfigurationRoot _configuration;

    public SettingsModule(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public void Register(IServiceCollection services, Action<DbContextOptionsBuilder>? dbContextOptionsBuilderAction = null)
    {
        services.AddDbContext<SettingsDbContext>(o =>
        {
            if (dbContextOptionsBuilderAction == null)
            {
                o.UseNpgsql(_configuration["ConnectionString"]);
            }
            else
            {
                dbContextOptionsBuilderAction(o);
            }
        });
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ISettingsApplicationService, SettingsApplicationService>();
    }

    public async Task MigrateDb(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await using var settingsDbContext = scope.ServiceProvider.GetRequiredService<SettingsDbContext>();
        await settingsDbContext.Database.MigrateAsync().ConfigureAwait(false);
    }
}