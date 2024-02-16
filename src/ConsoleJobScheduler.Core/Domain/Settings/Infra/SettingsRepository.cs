using ConsoleJobScheduler.Core.Domain.Settings.Model;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Settings.Infra;

public interface ISettingsRepository
{
    Task<List<SettingModel>> GetSettings(SettingCategory category);

    Task SaveSettings(SettingCategory category, IEnumerable<SettingModel> settings);
}

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly SettingsDbContext _settingsDbContext;

    public SettingsRepository(SettingsDbContext settingsDbContext)
    {
        _settingsDbContext = settingsDbContext ?? throw new ArgumentNullException(nameof(settingsDbContext));
    }

    public Task<List<SettingModel>> GetSettings(SettingCategory category)
    {
        return _settingsDbContext.Settings.Where(x => x.CategoryId == category).ToListAsync();
    }

    public Task SaveSettings(SettingCategory category, IEnumerable<SettingModel> settings)
    {
        return _settingsDbContext.Settings.BulkMergeAsync(settings, x => x.ColumnPrimaryKeyExpression = c => new { c.CategoryId, c.Name });
    }
}