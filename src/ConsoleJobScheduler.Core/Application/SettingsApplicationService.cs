using ConsoleJobScheduler.Core.Domain.Settings;
using ConsoleJobScheduler.Core.Domain.Settings.Model;

namespace ConsoleJobScheduler.Core.Application;

public interface ISettingsApplicationService
{
    Task<TSettings> GetSettings<TSettings>()
        where TSettings : ISettings, new();

    Task SaveSettings<TSettings>(TSettings settings)
        where TSettings : ISettings;
}

public sealed class SettingsApplicationService : ISettingsApplicationService
{
    private readonly ISettingsService _settingsService;

    public SettingsApplicationService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public Task<TSettings> GetSettings<TSettings>()
        where TSettings : ISettings, new()
    {
        return _settingsService.GetSettings<TSettings>();
    }

    public Task SaveSettings<TSettings>(TSettings settings)
        where TSettings : ISettings
    {
        return _settingsService.SaveSettings(settings);
    }
}