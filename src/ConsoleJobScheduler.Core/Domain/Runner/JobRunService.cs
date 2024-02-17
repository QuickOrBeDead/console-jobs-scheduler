using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;
using ConsoleJobScheduler.Core.Domain.Runner.Events;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.EMail.Model;
using ConsoleJobScheduler.Messaging;
using ConsoleJobScheduler.Messaging.Models;
using ConsoleJobScheduler.Core.Infra.EMail;
using MessagePipe;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public sealed class JobRunService : IJobRunService
{
    private readonly IJobRunRepository _jobRunRepository;
    private readonly IJobRunAttachmentRepository _jobRunAttachmentRepository;
    private readonly IJobPackageRepository _jobPackageRepository;
    private readonly IAsyncPublisher<JobConsoleLogMessageEvent> _jobConsoleLogMessagePublisher;
    private readonly IEmailSender _emailSender;

    private static readonly JsonSerializerOptions PackageManifestJsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

    public JobRunService(
        IJobRunRepository jobRunRepository, 
        IJobRunAttachmentRepository jobRunAttachmentRepository, 
        IJobPackageRepository jobPackageRepository,
        IAsyncPublisher<JobConsoleLogMessageEvent> jobConsoleLogMessagePublisher,
        IEmailSender emailSender)
    {
        _jobRunRepository = jobRunRepository;
        _jobRunAttachmentRepository = jobRunAttachmentRepository;
        _jobPackageRepository = jobPackageRepository;
        _jobConsoleLogMessagePublisher = jobConsoleLogMessagePublisher;
        _emailSender = emailSender;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public Task InsertJobRunLog(
        string jobRunId,
        string content,
        bool isError,
        CancellationToken cancellationToken = default)
    {
        var jobRunLog = new JobRunLog(jobRunId, content, isError);
        return _jobRunRepository.SaveJobRunLog(jobRunLog, cancellationToken);
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

    public async Task ProcessJobRunConsoleMessage(string jobRunId, string? data, bool isError, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(data))
        {
            if (isError)
            {
                await InsertJobRunLog(jobRunId, data, isError, cancellationToken);
                await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, data, isError), cancellationToken);
            }
            else
            {
                var consoleMessage = ConsoleMessageReader.ReadMessage(data);
                if (consoleMessage != null)
                {
                    switch (consoleMessage)
                    {
                        case { MessageType: ConsoleMessageType.Email, Message: EmailMessage emailMessage }:
                        {
                            await InsertJobRunLog(jobRunId, $"Sending email to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);
                            await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, $"Sending email to {emailMessage.To}", false), cancellationToken).ConfigureAwait(false);
                            var emailModel = EmailModel.Create(jobRunId, emailMessage.Subject, emailMessage.Body, emailMessage.To, emailMessage.CC, emailMessage.Bcc);
                            var emailMessageAttachments = emailMessage.Attachments;
                            for (var i = 0; i < emailMessageAttachments.Count; i++)
                            {
                                var attachment = emailMessageAttachments[i];
                                emailModel.AddAttachment(attachment.FileName, attachment.FileContent, attachment.ContentType);
                            }

                            await InsertJobRunEmail(emailModel, cancellationToken).ConfigureAwait(false);
                            await _emailSender.SendMailAsync(emailMessage, cancellationToken).ConfigureAwait(false);
                            await UpdateJobRunEmailIsSent(emailModel.Id, true, cancellationToken).ConfigureAwait(false);
                            await InsertJobRunLog(jobRunId, $"Email is sent to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);
                            await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, $"Email is sent to {emailMessage.To}", false), cancellationToken);
                            break;
                        }
                        case { MessageType: ConsoleMessageType.Log, Message: ConsoleLogMessage logMessage }:
                            await InsertJobRunLog(jobRunId, logMessage.Message, false, cancellationToken).ConfigureAwait(false);
                            await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, logMessage.Message, isError), cancellationToken).ConfigureAwait(false);
                            break;
                    }
                }
            }
        }
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