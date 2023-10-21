namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using System.Net.Mail;
using System.Net.Mime;
using System.Text;

using ConsoleJobScheduler.Messaging.Models;

public sealed class SmtpEmailSender : IEmailSender
{
    public async Task SendMailAsync(EmailMessage emailMessage)
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
                using var stream = new MemoryStream(Convert.FromBase64String(attachment.FileContent));
                mailMessage.Attachments.Add(new Attachment(stream, new ContentType(attachment.ContentType)));
            }

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}