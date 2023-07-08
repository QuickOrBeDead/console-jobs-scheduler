namespace ConsoleJobScheduler.WindowsService.Models;

public sealed class SchedulerMetadataModel
{
    public string? SchedulerName { get; set; }

    public string? SchedulerInstanceId { get; set; }

    public string? SchedulerType { get; set; }

    public bool SchedulerRemote { get; set; }

    public bool Started { get; set; }

    public bool InStandbyMode { get; set; }

    public bool Shutdown { get; set; }

    public string? JobStoreType { get; set; }

    public string? ThreadPoolType { get; set; }

    public int ThreadPoolSize { get; set; }

    public string? Version { get; set; }

    public string? Summary { get; set; }

    public DateTime? RunningSince { get; set; }
}