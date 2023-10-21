using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

public interface IEmailSender
{
    Task SendMailAsync(EmailMessage emailMessage);
}