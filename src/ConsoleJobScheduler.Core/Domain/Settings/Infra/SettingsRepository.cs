using ConsoleJobScheduler.Core.Domain.Settings.Model;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Settings.Infra;

public interface ISettingsRepository
{
    Task<List<Model.Settings>> GetSettings(SettingsCategory category);

    Task SaveSettings(SettingsCategory category, IEnumerable<Model.Settings> settings);
}

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly SettingsDbContext _settingsDbContext;

    public SettingsRepository(SettingsDbContext settingsDbContext)
    {
        _settingsDbContext = settingsDbContext ?? throw new ArgumentNullException(nameof(settingsDbContext));
    }

    public Task<List<Model.Settings>> GetSettings(SettingsCategory category)
    {
        return _settingsDbContext.Settings.Where(x => x.CategoryId == category).ToListAsync();
    }

    public async Task SaveSettings(SettingsCategory category, IEnumerable<Model.Settings> settings)
    {
        foreach (var setting in settings)
        {
            await _settingsDbContext.Settings.Upsert(setting)
                .On(v => new { v.CategoryId, v.Name })
                .WhenMatched(v => new Model.Settings
                {
                    Value = v.Value
                }).RunAsync();
        }
    }
}