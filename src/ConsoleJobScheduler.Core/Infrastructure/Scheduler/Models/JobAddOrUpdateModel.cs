using System.ComponentModel.DataAnnotations;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Models;

public sealed class JobAddOrUpdateModel
{
    [Required, MinLength(3)]
    public string? JobName { get; set; }

    [Required, MinLength(3)]
    public string? JobGroup { get; set; }

    public string? Description { get; set; }

    [RegularExpression(Constants.CronExpressionRegexPattern, ErrorMessage = "Cron Expression is not valid")]
    [Required, MinLength(10)]
    public string? CronExpression { get; set; }

    [Required]
    public string? Package { get; set; }

    public string? Parameters { get; set; }
}