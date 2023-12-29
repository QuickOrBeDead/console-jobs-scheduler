namespace ConsoleJobScheduler.Core.Api.Models;

public sealed class SchedulerStateRecordModel
{
    public TimeSpan CheckInInterval { get; set; }

    public DateTimeOffset CheckInTimestamp { get; set; }

    public string? SchedulerInstanceId { get; set; }
}