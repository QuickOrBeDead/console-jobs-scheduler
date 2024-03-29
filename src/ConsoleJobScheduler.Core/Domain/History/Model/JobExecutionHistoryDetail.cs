﻿namespace ConsoleJobScheduler.Core.Domain.History.Model;

public sealed class JobExecutionHistoryDetail : IJobExecutionHasLastSignalTimeout
{
    public string Id { get; set; } = null!;

    public string InstanceName { get; set; } = null!;

    public string JobName { get; set; } = null!;

    public string JobGroup { get; set; } = null!;

    public string TriggerName { get; set; } = null!;

    public string TriggerGroup { get; set; } = null!;

    public string PackageName { get; set; } = null!;

    public DateTime? ScheduledTime { get; set; }

    public DateTime FiredTime { get; set; }

    public TimeSpan? RunTime { get; set; }

    public bool HasError { get; set; }

    public string? ErrorMessage { get; set; }

    public bool Completed { get; set; }

    public bool Vetoed { get; set; }

    public DateTime LastSignalTime { get; set; }

    public bool HasSignalTimeout { get; set; }

    public string? CronExpressionDescription { get; set; }

    public void SetHasSignalTimeout(bool value)
    {
        HasSignalTimeout = value;
    }
}