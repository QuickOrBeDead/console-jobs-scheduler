namespace ConsoleJobScheduler.Core.Domain.History.Model;

public sealed class JobExecutionHistory
{
    public string Id { get; private set; }

    public string SchedulerName { get; private set; }

    public string InstanceName { get; private set; }

    public string PackageName { get; private set; }

    public string JobName { get; private set; }

    public string JobGroup { get; private set; }

    public string TriggerName { get; private set; }

    public string TriggerGroup { get; private set; }

    public DateTime? ScheduledTime { get; private set; }

    public DateTime FiredTime { get; private set; }
    
    public DateTime LastSignalTime { get; internal set; }

    public DateTime? NextFireTime { get; private set; }

    public string? CronExpressionString { get; private set; }

    public TimeSpan? RunTime { get; internal set; }

    public bool HasError { get; internal set; }

    public string? ErrorMessage { get; internal set; }

    public string? ErrorDetails { get; internal set; }

    public bool Completed { get; internal set; }

    public bool Vetoed { get; internal set; }

    internal JobExecutionHistory()
    {
    }

    public JobExecutionHistory(string id, string schedulerName, string instanceName, string? packageName,
        string jobName, string jobGroup, string triggerName, string triggerGroup, DateTime? scheduledTime,
        DateTime firedTime, DateTime lastSignalTime, DateTime? nextFireTime = null, string? cronExpressionString = null)
    {
        Id = id;
        SchedulerName = schedulerName;
        InstanceName = instanceName;
        PackageName = packageName ?? string.Empty;
        JobName = jobName;
        JobGroup = jobGroup;
        TriggerName = triggerName;
        TriggerGroup = triggerGroup;
        ScheduledTime = scheduledTime;
        FiredTime = firedTime;
        LastSignalTime = lastSignalTime;
        NextFireTime = nextFireTime;
        CronExpressionString = cronExpressionString;
    }

    public void SetException(Exception jobException)
    {
        HasError = true;
        ErrorMessage = jobException.Message;
        ErrorDetails = jobException.ToString();
    }

    public void SetVetoed()
    {
        Vetoed = true;
    }

    public void SetLastSignalTime(DateTime signalTime)
    {
        LastSignalTime = signalTime;
    }

    public void SetCompleted(TimeSpan runTime)
    {
        RunTime = runTime;
        Completed = true;
    }
}