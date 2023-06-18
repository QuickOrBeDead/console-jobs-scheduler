namespace ConsoleJobScheduler.WindowsService.Jobs;

public interface IEmailSender
{
    Task SendMailsAsync(string packageName, string jobRunId);

    string GetEmailsFolder(string packageName, string jobRunId);
}