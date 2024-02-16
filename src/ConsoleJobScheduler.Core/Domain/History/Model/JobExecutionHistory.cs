namespace ConsoleJobScheduler.Core.Domain.History.Model;

public sealed class JobExecutionHistory
{
    public string Id { get; set; }

    public string SchedulerName { get; set; }

    public string InstanceName { get; set; }

    public string? PackageName { get; set; }

    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public string TriggerName { get; set; }

    public string TriggerGroup { get; set; }

    public DateTimeOffset? ScheduledTime { get; set; }

    public DateTimeOffset FiredTime { get; set; }

    public DateTimeOffset? LastSignalTime { get; set; }

    public bool HasError { get; set; }

    public bool Completed { get; set; }

    public bool Vetoed { get; set; }

    public bool HasSignalTimeout { get; set; }

    public JobExecutionHistory(string id, string schedulerName, string instanceName, string? packageName, string jobName, string jobGroup, string triggerName, string triggerGroup, DateTimeOffset? scheduledTime, DateTimeOffset firedTime, DateTimeOffset? lastSignalTime)
    {
        Id = id;
        SchedulerName = schedulerName;
        InstanceName = instanceName;
        PackageName = packageName;
        JobName = jobName;
        JobGroup = jobGroup;
        TriggerName = triggerName;
        TriggerGroup = triggerGroup;
        ScheduledTime = scheduledTime;
        FiredTime = firedTime;
        LastSignalTime = lastSignalTime;
    }
}