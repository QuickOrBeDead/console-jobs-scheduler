﻿namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Models;

using System.Collections.ObjectModel;

using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;

public sealed class JobExecutionDetailModel
{
    public JobExecutionDetail Details { get; }

    public IList<string> Attachments { get; }

    public IList<LogLine> Logs { get; }

    public JobExecutionDetailModel(JobExecutionDetail details, IList<LogLine> logs, IList<string> attachments)
    {
        if (logs == null)
        {
            throw new ArgumentNullException(nameof(logs));
        }

        if (attachments == null)
        {
            throw new ArgumentNullException(nameof(attachments));
        }

        Details = details ?? throw new ArgumentNullException(nameof(details));
        Attachments = new ReadOnlyCollection<string>(attachments);
        Logs = new ReadOnlyCollection<LogLine>(logs);
    }
}