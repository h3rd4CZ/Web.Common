using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.Impl.Notification;
using RhDev.Common.Web.Core.Notification;

namespace RhDev.Customer.Component.Core.Impl.Notifications
{
    public class TestNotificationService : RazorTemplatedNotificationFacadeBase
    {
        public TestNotificationService(
            INotificationSender notificationSender,
            IDataStoreAcessRepositoryFactory dataStoreAcessRepositoryFactory,
            IOptionsSnapshot<CommonConfiguration> optionsSnapshot) : base(notificationSender, dataStoreAcessRepositoryFactory, optionsSnapshot)
        {
                
        }

        public async Task SendUserInfo(TestNotificationModel model, IdentityUser user)
        {
            var template = System.Text.Encoding.Default.GetString(Resource.TestEmail);

            await SendNotificationFor(user, model, new( template), notificationSystem: NotificationSystem.Email);
        }
    }
}
