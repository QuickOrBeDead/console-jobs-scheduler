using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Infra.EMail;

public interface IEmailSender
{
    Task SendMailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}