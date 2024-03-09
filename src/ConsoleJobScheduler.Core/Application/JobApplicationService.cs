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

    Task<Guid> SendEmailMessage(string jobRunId, int order, EmailMessage emailMessage, CancellationToken cancellationToken = default);

    Task<long> InsertJobRunLog(string jobRunId, int order, string content, bool isError, CancellationToken cancellationToken = default);
}

public sealed class JobApplicationService(IJobRunService jobRunService, IEmailSender emailSender)
    : IJobApplicationService
{
    public async Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var logs = await jobRunService.GetJobRunLogs(id).ConfigureAwait(false);
        var attachments = await jobRunService.GetJobRunAttachments(id).ConfigureAwait(false);

        transactionScope.Complete();

        return new JobExecutionDetailModel(logs, attachments);
    }

    public async Task<byte[]?> GetJobRunAttachmentContent(long id)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await  jobRunService.GetJobRunAttachmentContent(id).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public Task<long> InsertJobRunAttachment(AttachmentModel attachment, CancellationToken cancellationToken = default)
    {
        return jobRunService.InsertJobRunAttachment(attachment, cancellationToken);
    }

    public async Task SavePackage(string packageName, byte[] content)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadCommitted();
        await jobRunService.SavePackage(packageName, content).ConfigureAwait(false);
        transactionScope.Complete();
    }

    public async Task<List<string>> GetAllPackageNames()
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await jobRunService.GetAllPackageNames().ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<JobPackageDetails?> GetPackageDetails(string name)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await jobRunService.GetPackageDetails(name).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<PagedResult<JobPackageListItem>> ListPackages(int pageSize = 10, int page = 1)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await  jobRunService.ListPackages(pageSize, page).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<Guid> SendEmailMessage(string jobRunId, int order, EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        await jobRunService.InsertJobRunLog(jobRunId, order, $"Sending email to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);
        var emailModel = EmailModel.Create(jobRunId, emailMessage.Subject, emailMessage.Body, emailMessage.To, emailMessage.CC, emailMessage.Bcc);
        var emailMessageAttachments = emailMessage.Attachments;
        for (var i = 0; i < emailMessageAttachments.Count; i++)
        {
            var attachment = emailMessageAttachments[i];
            emailModel.AddAttachment(attachment.FileName, attachment.GetContentBytes(), attachment.ContentType);
        }

        await jobRunService.InsertJobRunEmail(emailModel, cancellationToken).ConfigureAwait(false);
        await emailSender.SendMailAsync(emailMessage, cancellationToken).ConfigureAwait(false);
        await jobRunService.UpdateJobRunEmailIsSent(emailModel.Id, true, cancellationToken).ConfigureAwait(false);
        await jobRunService.InsertJobRunLog(jobRunId, order, $"Email is sent to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);

        return emailModel.Id;
    }

    public Task<long> InsertJobRunLog(string jobRunId, int order, string content, bool isError, CancellationToken cancellationToken = default)
    {
        return jobRunService.InsertJobRunLog(jobRunId, order, content, isError, cancellationToken);
    }
}