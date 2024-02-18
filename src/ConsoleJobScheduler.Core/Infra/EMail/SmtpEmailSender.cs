using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.Settings.Model;
using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Infra.EMail;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly ISettingsApplicationService _settingsService;

    public SmtpEmailSender(ISettingsApplicationService settingsApplicationService)
    {
        _settingsService = settingsApplicationService ?? throw new ArgumentNullException(nameof(settingsApplicationService));
    }

    public async Task SendMailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        var smtpSettings = await _settingsService.GetSettings<SmtpSettings>().ConfigureAwait(false);
        using (var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port ?? 25))
        {
            smtpClient.EnableSsl = smtpSettings.EnableSsl;
            smtpClient.Credentials = new NetworkCredential(smtpSettings.UserName, smtpSettings.Password, smtpSettings.Domain);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings.From, smtpSettings.FromName),
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

                await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            }
            finally
            {
                streams.ForEach(x => x.Dispose());
            }
        }
    }
}