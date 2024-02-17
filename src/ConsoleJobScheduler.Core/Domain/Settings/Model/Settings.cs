namespace ConsoleJobScheduler.Core.Domain.Settings.Model;

public sealed class Settings
{
    public SettingsCategory CategoryId { get; set; }

    public required string Name { get; init; }

    public string? Value { get; init; }
}