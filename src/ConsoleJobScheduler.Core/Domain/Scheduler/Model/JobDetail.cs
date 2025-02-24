namespace ConsoleJobScheduler.Core.Domain.Scheduler.Model;

public sealed class JobDetail
{
    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public string? Description { get; set; }

    public string? CronExpression { get; set; }

    public string? Package { get; set; }

    public string? Parameters { get; set; }

    public string? CronExpressionDescription { get; set; }
}