using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;
using ConsoleJobScheduler.Core.Domain.Runner.Events;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using ConsoleJobScheduler.Core.Infra.EMail.Model;
using MessagePipe;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public interface IJobRunService
{
    Task<long> InsertJobRunLog(
        string jobRunId,
        int order,
        string content,
        bool isError,
        CancellationToken cancellationToken = default);

    Task<List<JobRunLogDetail>> GetJobRunLogs(string jobRunId);

    Task<long> InsertJobRunAttachment(AttachmentModel attachment, CancellationToken cancellationToken = default);

    Task InsertJobRunEmail(EmailModel email, CancellationToken cancellationToken = default);

    Task UpdateJobRunEmailIsSent(Guid id, bool isSent, CancellationToken cancellationToken = default);

    Task SavePackage(string packageName, byte[] content);

    Task<JobPackageRun?> GetPackageRun(string name, string tempRootPath);

    Task<List<string>> GetAllPackageNames();

    Task<JobPackageDetails?> GetPackageDetails(string name);

    Task<PagedResult<JobPackageListItem>> ListPackages(int pageSize = 10, int page = 1);

    Task<byte[]?> GetJobRunAttachmentContent(long id);

    Task<List<JobRunAttachmentInfo>> GetJobRunAttachments(string id);
}

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
    public async Task<long> InsertJobRunLog(
        string jobRunId,
        int order,
        string content,
        bool isError,
        CancellationToken cancellationToken = default)
    {
        var jobRunLog = JobRunLog.Create(jobRunId, order, content, isError);
        await _jobRunRepository.Add(jobRunLog, cancellationToken);
        await _jobRunRepository.SaveChanges(cancellationToken).ConfigureAwait(false);
        await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, content, isError), cancellationToken).ConfigureAwait(false);

        return jobRunLog.Id;
    }

    public Task<List<JobRunLogDetail>> GetJobRunLogs(string jobRunId)
    {
        return _jobRunRepository.QueryableAsNoTracking().Where(x => x.JobRunId == jobRunId)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.CreateDate)
            .Select(x => new JobRunLogDetail(x.Content, x.IsError, x.CreateDate)).ToListAsync();
    }

    public async Task<long> InsertJobRunAttachment(AttachmentModel attachment, CancellationToken cancellationToken = default)
    {
        var jobRunAttachment = new JobRunAttachment(attachment.JobRunId, attachment.FileContent, attachment.FileName, attachment.ContentType);
        await _jobRunAttachmentRepository.Add(jobRunAttachment, cancellationToken);
        await _jobRunAttachmentRepository.SaveChanges(cancellationToken);

        return jobRunAttachment.Id;
    }

    public Task<byte[]?> GetJobRunAttachmentContent(long id)
    {
        return _jobRunAttachmentRepository.FindAsNoTracking(id, x => x.Content);
    }

    public Task<List<JobRunAttachmentInfo>> GetJobRunAttachments(string id)
    {
        return _jobRunAttachmentRepository.QueryableAsNoTracking().Where(x => x.JobRunId == id)
            .Select(x => new JobRunAttachmentInfo
            {
                Id = x.Id,
                FileName = x.FileName
            }).ToListAsync();
    }

    public async Task InsertJobRunEmail(EmailModel email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var jobRunEmail = new JobRunEmail(email.Id, email.JobRunId, email.Subject, email.Body, email.To, email.CC, email.Bcc);
        foreach (var attachment in email.GetAttachments())
        {
            jobRunEmail.AddAttachment(attachment.FileName, attachment.FileContent, attachment.ContentType);
        }

        await _jobRunAttachmentRepository.Add(jobRunEmail, cancellationToken);
        await _jobRunAttachmentRepository.SaveChanges(cancellationToken);
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
        var manifestPackage = manifest.CreateJobPackage(packageName, content);

        var jobPackage = await _jobPackageRepository.Get(packageName).ConfigureAwait(false);
        if (jobPackage != null)
        {
            jobPackage.Name = manifestPackage.Name;
            jobPackage.Author = manifestPackage.Author;
            jobPackage.Version = manifestPackage.Version;
            jobPackage.Description = manifestPackage.Description;
            jobPackage.Content = manifestPackage.Content;
            jobPackage.FileName = manifestPackage.FileName;
            jobPackage.Arguments = manifestPackage.Arguments;
            jobPackage.CreateDate = manifestPackage.CreateDate;
        }
        else
        {
            jobPackage = manifestPackage;

            await _jobPackageRepository.Add(jobPackage).ConfigureAwait(false);
        }

        await _jobPackageRepository.SaveChanges().ConfigureAwait(false);
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<JobPackageRun?> GetPackageRun(string name, string tempRootPath)
    {
        var jobPackage = await _jobPackageRepository.Get(name).ConfigureAwait(false);

        return jobPackage?.CreateJobPackageRun(tempRootPath);
    }

    public Task<List<string>> GetAllPackageNames()
    {
        return _jobPackageRepository.QueryableAsNoTracking().Select(x => x.Name).ToListAsync();
    }

    public Task<JobPackageDetails?> GetPackageDetails(string name)
    {
        return _jobPackageRepository.FindAsNoTracking(name, x => new JobPackageDetails
        {
            Name = x.Name,
            ModifyDate = x.CreateDate
        });
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public Task<PagedResult<JobPackageListItem>> ListPackages(int pageSize = 10, int page = 1)
    {
        return _jobPackageRepository.QueryableAsNoTracking().List(pageSize, page,
            q => q.Select(
                x => new JobPackageListItem
                {
                    Name = x.Name
                }).OrderBy(x => x.Name));
    }

    private static async Task<JobPackageManifest> GetPackageManifest(byte[] packageContent)
    {
        JobPackageManifest jobPackageManifest;

        await using (var stream = new MemoryStream(packageContent))
        {
            using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                var manifestJsonEntry = zipArchive.GetEntry("manifest.json");
                if (manifestJsonEntry != null)
                {
                    using (var reader = new StreamReader(manifestJsonEntry.Open()))
                    {
                        jobPackageManifest = JsonSerializer.Deserialize<JobPackageManifest>(await reader.ReadToEndAsync().ConfigureAwait(false), PackageManifestJsonSerializerOptions) ?? throw new InvalidOperationException();
                        jobPackageManifest.Validate();
                    }
                }
                else
                {
                    throw new InvalidOperationException("manifest.json not found in package zip file.");
                }
            }
        }

        return jobPackageManifest;
    }
}