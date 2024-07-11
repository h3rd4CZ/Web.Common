using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.Notification.Providers;
using RhDev.Common.Web.Core.Notification;
using RhDev.Customer.Component.Core.Impl;
using RhDev.Customer.Component.Core.Impl.Notifications;
using Xunit.Abstractions;
using RhDev.Common.Web.Core.Test._setup;

namespace RhDev.Common.Web.Core.Test.Core
{
    public class NotificationTest : IntegrationTestBase
    {
        public NotificationTest(ITestOutputHelper testOutputHelper): base(testOutputHelper) 
        {
            RegisterMock(typeof(INotificationProvider<EmailNotification>), p => p.GetService<TestEmailNotificationProvider>()!);
        }
        
        [Fact]
        public async Task Base_Composition_Test()
        {
            var host = Host(new[] { TestCompositionDefinition.GetDefinition() });

            var services = host.Services;

            var notificationSvc = services.GetRequiredService<TestNotificationService>();
                        
            await notificationSvc.SendUserInfo(new(36, "Tomas", "Andersen", new List<string> { "c#", ".NET", "Azure" }), new Microsoft.AspNetCore.Identity.IdentityUser { Email = "user@email.com" });
        }   
    }
}
