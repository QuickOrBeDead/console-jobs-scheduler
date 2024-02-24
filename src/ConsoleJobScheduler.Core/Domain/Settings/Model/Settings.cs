namespace ConsoleJobScheduler.Core.Domain.Settings.Model;

public sealed class Settings
{
    public SettingsCategory CategoryId { get; set; }

    public string Name { get; set; }

    public string? Value { get; set; }
}