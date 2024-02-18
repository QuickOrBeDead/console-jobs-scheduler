namespace ConsoleJobScheduler.Core.Domain.Scheduler.Model;

public sealed class JobListItem
{
    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public string JobType { get; set; }

    public DateTime? LastFireTime { get; set; }

    public DateTime? NextFireTime { get; set; }

    public string? TriggerDescription { get; set; }
}