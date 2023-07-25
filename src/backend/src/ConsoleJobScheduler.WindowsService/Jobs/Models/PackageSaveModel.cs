namespace ConsoleJobScheduler.WindowsService.Jobs.Models;

public sealed class PackageSaveModel
{
    public string? Name { get; set; }

    public IFormFile? File { get; set; }
}