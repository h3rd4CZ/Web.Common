using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Configuration.ConfigEntities;
using RhDev.Common.Web.Core.Notification;
using RhDev.Common.Web.Core.Utils;
using System.Net.Mail;

namespace RhDev.Common.Web.Core.Impl.Notification.Providers
{
    public abstract class EmailNotificationSender
    {
        private readonly IOptionsSnapshot<CommonConfiguration> options;
        protected readonly ILogger logger;

        public EmailNotificationSender(IOptionsSnapshot<CommonConfiguration> options, ILogger logger)
        {
            this.options = options;
            this.logger = logger;
        }

        protected async Task SendMail(EmailNotification email, string? subject = default)
        {
            ValidateMailMessage(email);

            var config = options.Value?.SmtpServer;

            logger.LogTrace($"Initializing smtp client...");

            var recipient = email.recipient;
            var sender = config.Sender;
                        
            try
            {
                ValidateSmtpClientSettings(config);
                                                                
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.IsBodyHtml = true;
                    mailMessage.From = new MailAddress(sender);
                    mailMessage.Sender = new MailAddress(sender);
                    mailMessage.Subject = string.IsNullOrWhiteSpace(subject) ? config.Subject ?? string.Empty : subject;
                    mailMessage.Body = email.text;
                    mailMessage.To.Add(recipient.Email!);

                    var smtpClient = new SmtpClient
                    {
                        Host = config.Address,
                        Port = config.Port,
                        Timeout = config.TimeoutInSeconds * 1000,
                        UseDefaultCredentials = config.UseDefaultCredentials
                    };

                    if(config.Credential is not null and { ValidCredential : true})
                    {
                        smtpClient.Credentials = new System.Net.NetworkCredential(config.Credential.UserName, config.Credential.Password, config.Credential.Domain);
                    }

                    await smtpClient.SendMailAsync(mailMessage);
                }

                logger.LogInformation($"Email message for recipient : {email.recipient.Email} and User : {{@UserName}} sent successfully.", recipient.UserName);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"An error occured when sending mail message for recipient : {email.recipient.Email} and User : {{@UserName}}", recipient.UserName);

                if (!(config?.BypassOnFailure).Value) throw;
            }
        }

        private void ValidateSmtpClientSettings(SmtpServerConfiguration config)
        {
            Guard.NotNull(config, nameof(config));
            Guard.StringNotNullOrWhiteSpace(config.Address, nameof(config.Address));
            Guard.StringNotNullOrWhiteSpace(config.Sender, nameof(config.Sender));
        }

        private void ValidateMailMessage(EmailNotification email)
        {
            Guard.NotNull(email, nameof(email));
            Guard.StringNotNullOrWhiteSpace(email.text, nameof(email.text));
            Guard.NotNull(email.recipient, nameof(email.recipient));
            Guard.StringNotNullOrWhiteSpace(email.recipient.Email, nameof(email.recipient));
        }
    }
}
