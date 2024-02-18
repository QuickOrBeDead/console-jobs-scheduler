namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class SchedulerJobExecutionStatisticsModel
{
    public int TotalExecutedJobs { get; set; }

    public int TotalSucceededJobs { get; set; }

    public int TotalRunningJobs { get; set; }

    public int TotalFailedJobs { get; set; }

    public int TotalVetoedJobs { get; set; }
}