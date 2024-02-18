using ConsoleJobScheduler.Core.Domain.Settings.Infra;
using ConsoleJobScheduler.Core.Domain.Settings.Model;

namespace ConsoleJobScheduler.Core.Domain.Settings;

public interface ISettingsService
{
    Task<TSettings> GetSettings<TSettings>()
        where TSettings : ISettings, new();

    Task SaveSettings<TSettings>(TSettings settings)
        where TSettings : ISettings;
}

public sealed class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;

    public SettingsService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
    }

    public async Task<TSettings> GetSettings<TSettings>()
        where TSettings : ISettings, new()
    {
        var settings = new TSettings();
        var settingValues = (await _settingsRepository.GetSettings(settings.GetCategory()).ConfigureAwait(false))
            .ToDictionary(x => x.Name, x => x.Value);
        settings.Map(new SettingsData(settingValues));
        return settings;
    }

    public Task SaveSettings<TSettings>(TSettings settings)
        where TSettings : ISettings
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var data = settings.GetData();
        var settingModels = new List<Model.Settings>(data.Count);
        using (var dataEnumerator = data.GetEnumerator())
        {
            while (dataEnumerator.MoveNext())
            {
                settingModels.Add(new Model.Settings
                {
                    CategoryId = settings.GetCategory(),
                    Name = dataEnumerator.Current.Key,
                    Value = dataEnumerator.Current.Value
                });
            }
        }

        return _settingsRepository.SaveSettings(settings.GetCategory(), settingModels);
    }
}