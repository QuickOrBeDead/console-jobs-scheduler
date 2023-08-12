namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

public interface IEmailSender
{
    Task SendMailsAsync(string packageName, string jobRunId);

    string GetEmailsFolder(string packageName, string jobRunId);
}