using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core.Notification;
using RhDev.Common.Web.Core.Notification.Providers;

namespace RhDev.Common.Web.Core.Impl.Notification
{
    public class NotificationSender : INotificationSender
    {
        private readonly INotificationProvider<EmailNotification> emailProvider;
        private readonly INotificationProvider<SmsNotification> smsProvider;

        public NotificationSender(
            INotificationProvider<EmailNotification> emailProvider,
            INotificationProvider<SmsNotification> smsProvider)
        {
            this.emailProvider = emailProvider;
            this.smsProvider = smsProvider;
        }
        public async Task SendNotificationAsync(Core.Notification.Notification notification)
        {
            var sendTask = notification switch
            {
                EmailNotification email => emailProvider.SendAsync(email, email.subject),
                SmsNotification sms => smsProvider.SendAsync(sms),
                _ => throw new InvalidOperationException($"Unssuported notification type : {notification.GetType()}")
            };

            await sendTask;
        }
    }
}
