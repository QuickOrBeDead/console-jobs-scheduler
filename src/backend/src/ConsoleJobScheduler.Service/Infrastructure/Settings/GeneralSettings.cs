﻿namespace ConsoleJobScheduler.Service.Infrastructure.Settings;

using System.ComponentModel.DataAnnotations;

using ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

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