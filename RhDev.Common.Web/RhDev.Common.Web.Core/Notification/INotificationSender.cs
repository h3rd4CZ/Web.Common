using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core.Notification
{
    public interface INotificationSender : IAutoregisteredService
    {
        Task SendNotificationAsync(Notification notification);
    }
}
