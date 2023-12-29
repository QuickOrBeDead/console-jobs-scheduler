using System.ComponentModel.DataAnnotations;

using ConsoleJobScheduler.Core.Infrastructure.Scheduler;

namespace ConsoleJobScheduler.Core.Api.Models;

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