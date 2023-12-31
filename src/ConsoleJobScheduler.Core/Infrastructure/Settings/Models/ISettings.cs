﻿namespace ConsoleJobScheduler.Core.Infrastructure.Settings.Models;

public interface ISettings
{
    SettingCategory GetCategory();

    void Map(SettingsData data);

    SettingsData GetData();
}