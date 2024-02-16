using System.Collections.ObjectModel;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Domain.Runner.Model;

namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class JobExecutionDetailModel
{
    public JobExecutionDetail Details { get; }

    public IList<JobRunAttachmentInfo> Attachments { get; }

    public IList<JobRunLog> Logs { get; }

    public JobExecutionDetailModel(JobExecutionDetail details, IList<JobRunLog> logs, IList<JobRunAttachmentInfo> attachments)
    {
        ArgumentNullException.ThrowIfNull(logs);

        ArgumentNullException.ThrowIfNull(attachments);

        Details = details ?? throw new ArgumentNullException(nameof(details));
        Attachments = new ReadOnlyCollection<JobRunAttachmentInfo>(attachments);
        Logs = new ReadOnlyCollection<JobRunLog>(logs);
    }
}