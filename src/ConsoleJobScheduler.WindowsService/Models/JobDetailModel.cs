namespace ConsoleJobScheduler.WindowsService.Models;

using System.ComponentModel.DataAnnotations;

using ConsoleJobScheduler.WindowsService.Scheduler;

public sealed class JobDetailModel
{
    [Required, MinLength(3)]
    public string? JobName { get; set; }

    [Required, MinLength(3)]
    public string? JobGroup { get; set; }

    public string? Description { get; set; }

    [RegularExpression(Constants.CronExpressionRegexPattern, ErrorMessage = "Cron Expression is not valid")]
    [Required, MinLength(10)]
    public string? CronExpression { get; set; }

    public string? CronExpressionDescription { get; set; }
}