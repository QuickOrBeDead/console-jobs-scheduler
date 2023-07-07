namespace ConsoleJobScheduler.WindowsService.Jobs;

using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

using ConsoleJobScheduler.WindowsService.Jobs.Models;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly string _rootPath;

    public SmtpEmailSender(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootPath));
        }

        _rootPath = rootPath;
    }

    public async Task SendMailsAsync(string packageName, string jobRunId)
    {
        var emailsFolder = GetEmailsFolder(packageName, jobRunId);
        foreach (var emailJsonPath in Directory.EnumerateFiles(emailsFolder, "*.json", SearchOption.TopDirectoryOnly))
        {
            var jsonContent = await File.ReadAllTextAsync(emailJsonPath);
            await SendMailAsync(JsonSerializer.Deserialize<EmailMessage>(jsonContent));
        }
    }

    public string GetEmailsFolder(string packageName, string jobRunId)
    {
        var emailsFolder = Path.Combine(_rootPath, "Emails", packageName, jobRunId);
        if (!Directory.Exists(emailsFolder))
        {
            Directory.CreateDirectory(emailsFolder);
        }

        return emailsFolder;
    }

    private static async Task SendMailAsync(EmailMessage emailMessage)
    {
        using (var smtpClient = new SmtpClient("localhost", 25))
        {
            var mailMessage = new MailMessage
                                  {
                                      From = new MailAddress("test@test.com", "Test Mailer"),
                                      Subject = emailMessage.Subject,
                                      Body = emailMessage.Body,
                                      IsBodyHtml = true,
                                      BodyEncoding = Encoding.UTF8
                                  };
            if (!string.IsNullOrWhiteSpace(emailMessage.To))
            {
                mailMessage.To.Add(emailMessage.To);
            }

            if (!string.IsNullOrWhiteSpace(emailMessage.CC))
            {
                mailMessage.To.Add(emailMessage.CC);
            }

            if (!string.IsNullOrWhiteSpace(emailMessage.Bcc))
            {
                mailMessage.To.Add(emailMessage.Bcc);
            }

            foreach (var attachment in emailMessage.Attachments)
            {
                mailMessage.Attachments.Add(new Attachment(attachment.FileName, new ContentType(attachment.ContentType)));
            }

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}