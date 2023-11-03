namespace ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

public sealed class SettingModel
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public required string Name { get; init; }

    public string? Value { get; init; }

    public SettingCategoryModel? Category { get; set; }
}