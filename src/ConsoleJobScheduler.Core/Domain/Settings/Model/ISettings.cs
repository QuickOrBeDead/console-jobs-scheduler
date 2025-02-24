namespace ConsoleJobScheduler.Core.Domain.Settings.Model;

public interface ISettings
{
    SettingsCategory GetCategory();

    void Map(SettingsData data);

    SettingsData GetData();
}