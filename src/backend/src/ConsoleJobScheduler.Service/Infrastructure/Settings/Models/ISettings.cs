namespace ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

public interface ISettings
{
    int CategoryId { get; }

    void Map(SettingsData data);

    IDictionary<string, string?> GetData();
}