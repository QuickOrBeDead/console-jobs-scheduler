using System.ComponentModel.DataAnnotations;

using ConsoleJobScheduler.Core.Infrastructure.Settings.Models;

namespace ConsoleJobScheduler.Core.Infrastructure.Settings;

public sealed class GeneralSettings : ISettings
{
    [Display(Name = "Page Size")]
    [Range(10, 50)]
    public int? PageSize { get; set; }

    public SettingCategory GetCategory() => SettingCategory.General;

    public void Map(SettingsData data)
    {
        PageSize = data.GetInt(nameof(PageSize), 10);
    }

    public SettingsData GetData()
    {
        var result = new SettingsData();
        result.Set(nameof(PageSize), PageSize);
        return result;
    }
}