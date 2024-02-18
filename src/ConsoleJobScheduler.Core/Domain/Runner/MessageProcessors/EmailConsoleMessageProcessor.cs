using ConsoleJobScheduler.Core.Infra.EMail;
using ConsoleJobScheduler.Core.Infra.EMail.Model;
using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner.MessageProcessors;

public sealed class EmailConsoleMessageProcessor : IConsoleMessageProcessor
{
    private readonly IJobRunService _jobRunService;
    private readonly IEmailSender _emailSender;

    public EmailConsoleMessageProcessor(IJobRunService jobRunService, IEmailSender emailSender)
    {
        _jobRunService = jobRunService;
        _emailSender = emailSender;
    }

    public ConsoleMessageType MessageType => ConsoleMessageType.Email;

    public async Task ProcessMessage(string jobRunId, object message, CancellationToken cancellationToken = default)
    {
        var emailMessage = (EmailMessage)message;
        await _jobRunService.InsertJobRunLog(jobRunId, $"Sending email to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);
        var emailModel = EmailModel.Create(jobRunId, emailMessage.Subject, emailMessage.Body, emailMessage.To, emailMessage.CC, emailMessage.Bcc);
        var emailMessageAttachments = emailMessage.Attachments;
        for (var i = 0; i < emailMessageAttachments.Count; i++)
        {
            var attachment = emailMessageAttachments[i];
            emailModel.AddAttachment(attachment.FileName, attachment.FileContent, attachment.ContentType);
        }

        await _jobRunService.InsertJobRunEmail(emailModel, cancellationToken).ConfigureAwait(false);
        await _emailSender.SendMailAsync(emailMessage, cancellationToken).ConfigureAwait(false);
        await _jobRunService.UpdateJobRunEmailIsSent(emailModel.Id, true, cancellationToken).ConfigureAwait(false);
        await _jobRunService.InsertJobRunLog(jobRunId, $"Email is sent to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);
    }
}