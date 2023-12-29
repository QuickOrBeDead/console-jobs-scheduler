namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs.Models;

public sealed class PackageDetailsModel
{
    public string? Name { get; set; }

    public DateTime? ModifyDate { get; set; }
}