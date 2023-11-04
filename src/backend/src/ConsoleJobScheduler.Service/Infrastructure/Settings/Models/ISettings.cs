namespace ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

public interface ISettings
{
    SettingCategory GetCategory();

    void Map(SettingsData data);

    SettingsData GetData();
}