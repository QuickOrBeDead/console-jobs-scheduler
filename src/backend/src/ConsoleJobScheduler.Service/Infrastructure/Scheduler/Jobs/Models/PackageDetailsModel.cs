namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;

public sealed class PackageDetailsModel
{
    public string? Name { get; set; }

    public string? Path { get; set; }

    public DateTime? ModifyDate { get; set; }
}