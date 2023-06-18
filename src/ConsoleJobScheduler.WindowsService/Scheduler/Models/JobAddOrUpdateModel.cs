﻿namespace ConsoleJobScheduler.WindowsService.Scheduler.Models;

public sealed class JobAddOrUpdateModel
{
    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public string? Description { get; set; }

    public string CronExpression { get; set; }

    public string Package { get; set; }

    public string Parameters { get; set; }
}