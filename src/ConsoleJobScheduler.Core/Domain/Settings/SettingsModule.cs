using ConsoleJobScheduler.Core.Domain.Settings.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Domain.Settings;

public sealed class SettingsModule
{
    private readonly IConfigurationRoot _configuration;

    public SettingsModule(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public void Register(IServiceCollection services)
    {
        services.AddDbContext<SettingsDbContext>(o => o.UseNpgsql(_configuration["ConnectionString"]));
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ISettingsService, SettingsService>();
    }
}