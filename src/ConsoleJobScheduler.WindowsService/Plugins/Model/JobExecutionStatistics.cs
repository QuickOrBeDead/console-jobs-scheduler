namespace ConsoleJobScheduler.WindowsService.Plugins.Model;

public sealed class JobExecutionStatistics
{
    public int TotalExecutedJobs { get; set; }

    public int TotalSucceededJobs { get; set; }

    public int TotalRunningJobs { get; set; }

    public int TotalFailedJobs { get; set; }

    public int TotalVetoedJobs { get; set; }
}