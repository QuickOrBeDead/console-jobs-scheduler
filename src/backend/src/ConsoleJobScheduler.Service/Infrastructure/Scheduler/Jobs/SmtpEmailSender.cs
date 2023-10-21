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

            var attachments = emailMessage.Attachments;
            var streams = new List<MemoryStream>(attachments.Count);
            try
            {
                for (var i = 0; i < attachments.Count; i++)
                {
                    var attachment = attachments[i];
                    var stream = new MemoryStream(Convert.FromBase64String(attachment.FileContent));
                    mailMessage.Attachments.Add(new Attachment(stream, new ContentType(attachment.ContentType)) { Name = attachment.FileName ?? $"attachment_{i}" });

                    streams.Add(stream);
                }

                await smtpClient.SendMailAsync(mailMessage);
            }
            finally
            {
                streams.ForEach(x => x.Dispose());
            }
        }
    }
}