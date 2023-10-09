namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;

public sealed class JobExecutionDetail
{
    public string InstanceName { get; set; }

    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public string TriggerName { get; set; }

    public string TriggerGroup { get; set; }

    public string PackageName { get; set; }

    public DateTime ScheduledTime { get; set; }

    public DateTime FiredTime { get; set; }

    public TimeSpan? RunTime { get; set; }

    public bool HasError { get; set; }

    public string? ErrorMessage { get; set; }

    public bool Completed { get; set; }

    public bool Vetoed { get; set; }
}