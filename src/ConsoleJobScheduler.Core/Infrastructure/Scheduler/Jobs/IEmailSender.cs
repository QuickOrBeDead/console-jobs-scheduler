using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs;

public interface IEmailSender
{
    Task SendMailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}