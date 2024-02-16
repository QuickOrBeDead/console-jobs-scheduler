using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.EMail.Model;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public interface IJobRunService
{
    Task InsertJobRunLog(string jobRunId, string content, bool isError, CancellationToken cancellationToken = default);

    Task InsertJobRunAttachment(AttachmentModel attachment, CancellationToken cancellationToken = default);

    Task InsertJobRunEmail(EmailModel email, CancellationToken cancellationToken = default);

    Task UpdateJobRunEmailIsSent(Guid id, bool isSent, CancellationToken cancellationToken = default);

    Task<JobPackageRun?> GetPackageRun(string name, string tempRootPath);

    Task SavePackage(string packageName, byte[] content);
    Task ProcessJobRunConsoleMessage(string jobRunId, string? data, bool isError, CancellationToken cancellationToken);
}