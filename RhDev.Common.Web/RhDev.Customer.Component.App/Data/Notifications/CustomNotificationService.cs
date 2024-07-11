using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.Impl.Notification;
using RhDev.Common.Web.Core.Notification;

namespace RhDev.Customer.Component.App.Data.Notifications
{
    public class CustomNotificationService : RazorTemplatedNotificationFacadeBase
    {
        public CustomNotificationService(
            INotificationSender notificationSender,
            IDataStoreAcessRepositoryFactory dataStoreAcessRepositoryFactory,
            IOptionsSnapshot<CommonConfiguration> optionsSnapshot) : base(notificationSender, dataStoreAcessRepositoryFactory, optionsSnapshot)
        {

        }

        public async Task SendWeatherInfo(WeatherNotificationModel model, IdentityUser user)
        {
            var header = "<style>table tr th{color:red}</style>";
            var template = System.Text.Encoding.Default.GetString(Resource.WeatherNotificationTemplate);
            var footer = "<br /><br /><i>Na tento email neodpovídejte</i>";

            await SendNotificationFor(user, model, new (template, header, footer), notificationSystem: NotificationSystem.Email);
        }
    }
}
