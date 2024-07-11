using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RazorEngineCore;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.Extensions;
using RhDev.Common.Web.Core.Notification;
using RhDev.Common.Web.Core.Utils;
using System.Globalization;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace RhDev.Common.Web.Core.Impl.Notification
{
    public abstract class RazorTemplatedNotificationFacadeBase
    {
        private readonly INotificationSender notificationSender;
        protected readonly IDataStoreAcessRepositoryFactory dataStoreAcessRepositoryFactory;
        protected readonly IOptionsSnapshot<CommonConfiguration> options;             
                
        protected RazorTemplatedNotificationFacadeBase(
            INotificationSender notificationSender,
            IDataStoreAcessRepositoryFactory dataStoreAcessRepositoryFactory,
            IOptionsSnapshot<CommonConfiguration> options)
        {
            this.notificationSender = notificationSender;
            this.dataStoreAcessRepositoryFactory = dataStoreAcessRepositoryFactory;
            this.options = options;
        }  
             
        protected async Task SendNotificationFor<TModel>(
            IdentityUser user, TModel model, MailTemplate mailTemplate, CultureInfo culture = default!,
            NotificationSystem notificationSystem = NotificationSystem.Email, string subject = default!,
            Action<IRazorEngineCompilationOptionsBuilder> userBuilder = default!)
        {
            var body = await ExecuteTemplate(mailTemplate.body, model, userBuilder);
            var header = await ExecuteTemplate(mailTemplate.header, model, userBuilder);
            var footer = await ExecuteTemplate(mailTemplate.footer, model, userBuilder);

            var mailText = $"{header}{body}{footer}";

            var notification = CreateNotificationFor(user, mailText, culture, notificationSystem,subject);

            await notificationSender.SendNotificationAsync(notification);
        }
                
        private Core.Notification.Notification CreateNotificationFor(IdentityUser user, string text, CultureInfo culture = default, NotificationSystem notificationSystem = NotificationSystem.Email, string subject = default)
        {
            var system = notificationSystem;

            culture ??= Thread.CurrentThread.CurrentUICulture;
                        
            Guard.StringNotNullOrWhiteSpace(text, nameof(text), $"Email body is empty");
                                                          
            return CreateNotification(text, user, notificationSystem, subject);
        }

        private Core.Notification.Notification CreateNotification(string text, IdentityUser recipient, NotificationSystem system, string subject)
        {
            switch (system)
            {
                case NotificationSystem.Email:
                    {
                        if (recipient is null || string.IsNullOrWhiteSpace(recipient.Email) || !recipient.Email.IsValidEmailAddress()) throw new InvalidOperationException("Recipient email address invalid");

                        return new EmailNotification(recipient, text, subject);
                    }
                case NotificationSystem.Sms:
                    {
                        if (recipient is null || string.IsNullOrWhiteSpace(recipient.PhoneNumber) || !recipient.PhoneNumber.IsValidPhoneNumber()) throw new InvalidOperationException("Phone number is not valid");

                        return new SmsNotification(recipient, text, subject);
                    }

                default: throw new InvalidOperationException("An unknown system detected");
            }
        }

        private async Task<string?> ExecuteTemplate<TModel>(string? template, TModel model, Action<IRazorEngineCompilationOptionsBuilder> userBuilder = default!)
        {
            if (string.IsNullOrWhiteSpace(template)) return template;

            IRazorEngine razorEngine = new RazorEngine();

            IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>> tmpl = razorEngine.Compile<RazorEngineTemplateBase<TModel>>(template, b =>
            {
                b.AddAssemblyReference(typeof(TModel));
                b.AddAssemblyReferenceByName("System.Collections");

                if(userBuilder is not null) userBuilder(b);
            });

            return await tmpl.RunAsync(a => a.Model = model);
        }
    }
}
