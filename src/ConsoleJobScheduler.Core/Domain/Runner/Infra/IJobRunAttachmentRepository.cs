using ConsoleJobScheduler.Core.Domain.Runner.Model;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public interface IJobRunAttachmentRepository
{
    Task SaveJobRunEmail(JobRunEmail email, CancellationToken cancellationToken = default);

    Task UpdateJobRunEmailIsSent(Guid id, bool isSent, CancellationToken cancellationToken = default);

    Task<byte[]?> GetJobRunAttachmentContent(long id);

    Task InsertJobRunAttachment(JobRunAttachment attachment, CancellationToken cancellationToken = default);

    Task<List<JobRunAttachmentInfo>> GetJobRunAttachments(string id);
}