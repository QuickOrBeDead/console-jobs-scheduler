namespace ConsoleJobScheduler.Core.Domain.History.Model;

public sealed class JobExecutionHistoryListItem : IJobExecutionHasLastSignalTimeout
{
    public string Id { get; init; } = null!;

    public string JobName { get; set; } = null!;

    public string JobGroup { get; set; } = null!;

    public string TriggerName { get; set; } = null!;

    public string TriggerGroup { get; set; } = null!;

    public DateTime? ScheduledTime { get; set; }

    public DateTime FiredTime { get; set; }

    public DateTime LastSignalTime { get; init; }

    public DateTime? NextFireTime { get; set; }

    public TimeSpan? RunTime { get; set; }

    public bool HasError { get; set; }

    public bool Completed { get; init; }

    public bool Vetoed { get; init; }

    public bool HasSignalTimeout { get; set; }

    public void SetHasSignalTimeout(bool value)
    {
        HasSignalTimeout = value;
    }
}