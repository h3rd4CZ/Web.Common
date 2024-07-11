using RhDev.Common.Web.Core.Notification.Providers;
using RhDev.Common.Web.Core.Notification;
using Microsoft.Extensions.Logging;

namespace RhDev.Common.Web.Core.Test._setup
{
    public class TestEmailNotificationProvider : INotificationProvider<EmailNotification>
    {
        private readonly ILogger<TestEmailNotificationProvider> logger;

        public TestEmailNotificationProvider(ILogger<TestEmailNotificationProvider> logger)
        {
            this.logger = logger;
        }
        public Task SendAsync(EmailNotification notification, string? subject = null)
        {
            var recipient = notification.recipient;
            var text = notification.text;

            logger.LogInformation($"Sending email notification to : {recipient?.Email}::");
            logger.LogInformation("----------------------------");
            logger.LogInformation(text);
            logger.LogInformation("----------------------------");

            return Task.CompletedTask;
        }
    }
}
