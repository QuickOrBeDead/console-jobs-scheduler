namespace ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

public sealed class SettingCategoryModel
{
    public int Id { get; set; }

    public required string Name { get; init; }
}