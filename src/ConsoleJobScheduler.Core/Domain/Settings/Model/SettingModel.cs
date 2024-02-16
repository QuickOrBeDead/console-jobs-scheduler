namespace ConsoleJobScheduler.Core.Domain.Settings.Model;

public sealed class SettingModel
{
    public SettingCategory CategoryId { get; set; }

    public required string Name { get; init; }

    public string? Value { get; init; }
}