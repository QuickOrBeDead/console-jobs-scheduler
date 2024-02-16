namespace ConsoleJobScheduler.Core.Domain.Settings.Model;

public interface ISettings
{
    SettingCategory GetCategory();

    void Map(SettingsData data);

    SettingsData GetData();
}