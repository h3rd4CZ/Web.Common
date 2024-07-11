using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core.Notification.Providers
{
    public interface INotificationProvider<TNotification> : IAutoregisteredService where TNotification : Notification
    {
        Task SendAsync(TNotification notification, string? subject = default);
    }
}
