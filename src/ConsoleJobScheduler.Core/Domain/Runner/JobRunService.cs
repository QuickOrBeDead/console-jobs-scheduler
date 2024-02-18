using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;
using ConsoleJobScheduler.Core.Domain.Runner.Events;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.EMail.Model;
using MessagePipe;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public sealed class JobRunService : IJobRunService
{
    private readonly IJobRunRepository _jobRunRepository;
    private readonly IJobRunAttachmentRepository _jobRunAttachmentRepository;
    private readonly IJobPackageRepository _jobPackageRepository;
    private readonly IAsyncPublisher<JobConsoleLogMessageEvent> _jobConsoleLogMessagePublisher;

    private static readonly JsonSerializerOptions PackageManifestJsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

    public JobRunService(
        IJobRunRepository jobRunRepository,
        IJobRunAttachmentRepository jobRunAttachmentRepository,
        IJobPackageRepository jobPackageRepository,
        IAsyncPublisher<JobConsoleLogMessageEvent> jobConsoleLogMessagePublisher)
    {
        _jobRunRepository = jobRunRepository;
        _jobRunAttachmentRepository = jobRunAttachmentRepository;
        _jobPackageRepository = jobPackageRepository;
        _jobConsoleLogMessagePublisher = jobConsoleLogMessagePublisher;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task InsertJobRunLog(
        string jobRunId,
        string content,
        bool isError,
        CancellationToken cancellationToken = default)
    {
        var jobRunLog = new JobRunLog(jobRunId, content, isError);
        await _jobRunRepository.SaveJobRunLog(jobRunLog, cancellationToken);
        await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, content, isError), cancellationToken).ConfigureAwait(false);
    }

    public Task InsertJobRunAttachment(AttachmentModel attachment, CancellationToken cancellationToken = default)
    {
        var jobRunAttachment = new JobRunAttachment(attachment.JobRunId, attachment.FileContent, attachment.FileName, attachment.ContentType);
        return _jobRunAttachmentRepository.InsertJobRunAttachment(jobRunAttachment, cancellationToken);
    }

    public Task InsertJobRunEmail(EmailModel email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var jobRunEmail = new JobRunEmail(email.JobRunId, email.Subject, email.Body, email.To, email.CC, email.Bcc);
        foreach (var attachment in email.GetAttachments())
        {
            jobRunEmail.AddAttachment(attachment.FileName, attachment.FileContent, attachment.ContentType);
        }

        return _jobRunAttachmentRepository.SaveJobRunEmail(jobRunEmail, cancellationToken);
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public Task UpdateJobRunEmailIsSent(Guid id, bool isSent, CancellationToken cancellationToken = default)
    {
        return _jobRunAttachmentRepository.UpdateJobRunEmailIsSent(id, isSent, cancellationToken);
    }

    public async Task SavePackage(string packageName, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageName));
        }

        var manifest = await GetPackageManifest(content).ConfigureAwait(false);
        var jobPackage = manifest.CreateJobPackage(packageName, content);
        await _jobPackageRepository.SavePackage(jobPackage);
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<JobPackageRun?> GetPackageRun(string name, string tempRootPath)
    {
        var jobPackage = await _jobPackageRepository.GetByName(name);

        return jobPackage?.CreateJobPackageRun(tempRootPath);
    }

    private static async Task<PackageManifest> GetPackageManifest(byte[] packageContent)
    {
        PackageManifest packageManifest;

        await using (var stream = new MemoryStream(packageContent))
        {
            using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                var manifestJsonEntry = zipArchive.GetEntry("manifest.json");
                if (manifestJsonEntry != null)
                {
                    using (var reader = new StreamReader(manifestJsonEntry.Open()))
                    {
                        packageManifest = JsonSerializer.Deserialize<PackageManifest>(await reader.ReadToEndAsync().ConfigureAwait(false), PackageManifestJsonSerializerOptions) ?? throw new InvalidOperationException();
                        packageManifest.Validate();
                    }
                }
                else
                {
                    throw new InvalidOperationException("manifest.json not found in package zip file.");
                }
            }
        }

        return packageManifest;
    }
}