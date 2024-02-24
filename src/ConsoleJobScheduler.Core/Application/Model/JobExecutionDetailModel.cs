using System.Collections.ObjectModel;
using ConsoleJobScheduler.Core.Domain.Runner.Model;

namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class JobExecutionDetailModel
{
    public IList<JobRunAttachmentInfo> Attachments { get; }

    public IList<JobRunLog> Logs { get; }

    public JobExecutionDetailModel(IList<JobRunLog> logs, IList<JobRunAttachmentInfo> attachments)
    {
        ArgumentNullException.ThrowIfNull(logs);

        ArgumentNullException.ThrowIfNull(attachments);
        
        Attachments = new ReadOnlyCollection<JobRunAttachmentInfo>(attachments);
        Logs = new ReadOnlyCollection<JobRunLog>(logs);
    }
}