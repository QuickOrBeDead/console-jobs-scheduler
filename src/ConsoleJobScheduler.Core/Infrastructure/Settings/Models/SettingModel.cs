namespace ConsoleJobScheduler.Core.Infrastructure.Settings.Models;

public sealed class SettingModel
{
    public SettingCategory CategoryId { get; set; }

    public required string Name { get; init; }

    public string? Value { get; init; }
}