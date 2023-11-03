namespace ConsoleJobScheduler.Service.Infrastructure.Settings.Service;

using ConsoleJobScheduler.Service.Infrastructure.Settings.Data;
using ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

using Microsoft.EntityFrameworkCore;

public interface ISettingsService
{
    Task<TSettings> GetSettings<TSettings>()
        where TSettings : ISettings, new();

    Task SaveSettings<TSettings>(TSettings settings)
        where TSettings : ISettings;
}

public sealed class SettingsService : ISettingsService
{
    private readonly SettingsDbContext _settingsDbContext;

    public SettingsService(SettingsDbContext settingsDbContext)
    {
        _settingsDbContext = settingsDbContext ?? throw new ArgumentNullException(nameof(settingsDbContext));
    }

    public async Task<TSettings> GetSettings<TSettings>()
        where TSettings : ISettings, new()
    {
        var settings = new TSettings();
        var settingValues = (await _settingsDbContext.Settings.Where(x => x.CategoryId == settings.CategoryId).ToListAsync())
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
        IList<SettingModel> settingModels = new List<SettingModel>(data.Count);
        using (var dataEnumerator = data.GetEnumerator())
        {
            settingModels.Add(new SettingModel
                                  {
                                      CategoryId = settings.CategoryId,
                                      Name = dataEnumerator.Current.Key,
                                      Value = dataEnumerator.Current.Value
                                  });
        }

        return _settingsDbContext.Settings.BulkUpdateAsync(settingModels, x => x.ColumnPrimaryKeyExpression = c => new { c.CategoryId, c.Name });
    }
}