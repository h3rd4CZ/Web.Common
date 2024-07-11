using RhDev.Common.Web.Core.Composition;
using System.Globalization;

namespace RhDev.Common.Web.Core.Notification
{
    public interface INotificationTextReader : IAutoregisteredService
    {
        string ReadText(NotificationSystem system, string key, CultureInfo ci, params string[] parameters);
    }
}
