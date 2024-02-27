using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using ConsoleJobScheduler.Core.Infra.EMail;
using ConsoleJobScheduler.Core.Infra.EMail.Model;
using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Application;

public interface IJobApplicationService
{
    Task<byte[]?> GetJobRunAttachmentContent(long id);

    Task<long> InsertJobRunAttachment(AttachmentModel attachment, CancellationToken cancellationToken = default);

    Task SavePackage(string packageName, byte[] content);

    Task<List<string>> GetAllPackageNames();

    Task<JobPackageDetails?> GetPackageDetails(string name);

    Task<PagedResult<JobPackageListItem>> ListPackages(int pageSize = 10, int page = 1);

    Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id);

    Task<Guid> SendEmailMessage(string jobRunId, EmailMessage emailMessage, CancellationToken cancellationToken = default);

    Task<long> InsertJobRunLog(string jobRunId, string content, bool isError, CancellationToken cancellationToken = default);
}

public sealed class JobApplicationService : IJobApplicationService
{
    private readonly IJobRunService _jobRunService;
    private readonly IEmailSender _emailSender;

    public JobApplicationService(IJobRunService jobRunService, IEmailSender emailSender)
    {
        _jobRunService = jobRunService;
        _emailSender = emailSender;
    }

    public async Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var logs = await _jobRunService.GetJobRunLogs(id).ConfigureAwait(false);
        var attachments = await _jobRunService.GetJobRunAttachments(id).ConfigureAwait(false);

        transactionScope.Complete();

        return new JobExecutionDetailModel(logs, attachments);
    }

    public async Task<byte[]?> GetJobRunAttachmentContent(long id)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await  _jobRunService.GetJobRunAttachmentContent(id).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public Task<long> InsertJobRunAttachment(AttachmentModel attachment, CancellationToken cancellationToken = default)
    {
        return _jobRunService.InsertJobRunAttachment(attachment, cancellationToken);
    }

    public async Task SavePackage(string packageName, byte[] content)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadCommitted();
        await _jobRunService.SavePackage(packageName, content).ConfigureAwait(false);
        transactionScope.Complete();
    }

    public async Task<List<string>> GetAllPackageNames()
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await _jobRunService.GetAllPackageNames().ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<JobPackageDetails?> GetPackageDetails(string name)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await _jobRunService.GetPackageDetails(name).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<PagedResult<JobPackageListItem>> ListPackages(int pageSize = 10, int page = 1)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await  _jobRunService.ListPackages(pageSize, page).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<Guid> SendEmailMessage(string jobRunId, EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        await _jobRunService.InsertJobRunLog(jobRunId, $"Sending email to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);
        var emailModel = EmailModel.Create(jobRunId, emailMessage.Subject, emailMessage.Body, emailMessage.To, emailMessage.CC, emailMessage.Bcc);
        var emailMessageAttachments = emailMessage.Attachments;
        for (var i = 0; i < emailMessageAttachments.Count; i++)
        {
            var attachment = emailMessageAttachments[i];
            emailModel.AddAttachment(attachment.FileName, attachment.GetContentBytes(), attachment.ContentType);
        }

        await _jobRunService.InsertJobRunEmail(emailModel, cancellationToken).ConfigureAwait(false);
        await _emailSender.SendMailAsync(emailMessage, cancellationToken).ConfigureAwait(false);
        await _jobRunService.UpdateJobRunEmailIsSent(emailModel.Id, true, cancellationToken).ConfigureAwait(false);
        await _jobRunService.InsertJobRunLog(jobRunId, $"Email is sent to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);

        return emailModel.Id;
    }

    public Task<long> InsertJobRunLog(string jobRunId, string content, bool isError, CancellationToken cancellationToken = default)
    {
        return _jobRunService.InsertJobRunLog(jobRunId, content, isError, cancellationToken);
    }
}