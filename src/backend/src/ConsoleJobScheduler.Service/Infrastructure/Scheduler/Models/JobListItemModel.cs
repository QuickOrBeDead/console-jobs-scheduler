namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Models;

public sealed class JobListItemModel
{
    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public string JobType { get; set; }

    public DateTime? LastFireTime { get; set; }

    public DateTime? NextFireTime { get; set; }

    public string? TriggerDescription { get; set; }
}