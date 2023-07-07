namespace ConsoleJobScheduler.WindowsService.Plugins.Model;

public sealed class JobExecutionHistory
{
    public string Id { get; set; }

    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public string TriggerName { get; set; }

    public string TriggerGroup { get; set; }

    public DateTime ScheduledTime { get; set; }

    public DateTime FiredTime { get; set; }

    public DateTime? NextFireTime { get; set; }

    public TimeSpan? RunTime { get; set; }

    public bool HasError { get; set; }

    public bool Completed { get; set; }

    public bool Vetoed { get; set; }
}