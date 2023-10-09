namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;

public sealed class LogLine
{
    public bool IsError { get; set; }

    public string? Message { get; set; }
}