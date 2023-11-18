namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Models;

using System.Collections.ObjectModel;

using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;

public sealed class JobExecutionDetailModel
{
    public JobExecutionDetail Details { get; }

    public IList<AttachmentInfoModel> Attachments { get; }

    public IList<LogLine> Logs { get; }

    public JobExecutionDetailModel(JobExecutionDetail details, IList<LogLine> logs, IList<AttachmentInfoModel> attachments)
    {
        ArgumentNullException.ThrowIfNull(logs);

        ArgumentNullException.ThrowIfNull(attachments);

        Details = details ?? throw new ArgumentNullException(nameof(details));
        Attachments = new ReadOnlyCollection<AttachmentInfoModel>(attachments);
        Logs = new ReadOnlyCollection<LogLine>(logs);
    }
}