using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Notification;
using RhDev.Common.Web.Core.Notification.Providers;

namespace RhDev.Common.Web.Core.Impl.Notification.Providers
{
    public class EmailNotificationProvider : EmailNotificationSender, INotificationProvider<EmailNotification>
    {
        public EmailNotificationProvider(
            ILogger<EmailNotificationProvider> logger,
            IOptionsSnapshot<CommonConfiguration> options) : base(options, logger) { }

        public async Task SendAsync(EmailNotification notification, string? subject = default) => await SendMail(notification, subject);
    }
}
