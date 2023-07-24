namespace ConsoleJobScheduler.WindowsService.Jobs.Models;

public sealed class PackageDetailsModel
{
    public string? Name { get; set; }

    public string? Path { get; set; }

    public DateTime? ModifyDate { get; set; }
}