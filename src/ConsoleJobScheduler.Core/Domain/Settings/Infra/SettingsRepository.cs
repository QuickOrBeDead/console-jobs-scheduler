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

    public Task SaveSettings(SettingsCategory category, IEnumerable<Model.Settings> settings)
    {
        return _settingsDbContext.Settings.BulkMergeAsync(settings, x => x.ColumnPrimaryKeyExpression = c => new { c.CategoryId, c.Name });
    }
}