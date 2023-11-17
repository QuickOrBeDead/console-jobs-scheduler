namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;

public sealed class JobExecutionDetail : IJobExecutionHasLastSignalTimeout
{
    public string Id { get; set; } = default!;

    public string InstanceName { get; set; } = default!;

    public string JobName { get; set; } = default!;

    public string JobGroup { get; set; } = default!;

    public string TriggerName { get; set; } = default!;

    public string TriggerGroup { get; set; } = default!;

    public string PackageName { get; set; } = default!;

    public DateTime ScheduledTime { get; set; }

    public DateTime FiredTime { get; set; }

    public TimeSpan? RunTime { get; set; }

    public bool HasError { get; set; }

    public string? ErrorMessage { get; set; }

    public bool Completed { get; set; }

    public bool Vetoed { get; set; }

    public DateTime LastSignalTime { get; set; }

    public bool HasSignalTimeout { get; set; }

    public string? CronExpressionDescription { get; set; }
}