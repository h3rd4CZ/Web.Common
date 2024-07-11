using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Configuration.ConfigEntities;
using RhDev.Common.Web.Core.Notification;
using RhDev.Common.Web.Core.Notification.Providers;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Notification.Providers
{
    public class DaktelaSmsNotificationProvider : INotificationProvider<SmsNotification>
    {
        private readonly ILogger<DaktelaSmsNotificationProvider> logger;
        private readonly IOptionsSnapshot<CommonConfiguration> options;

        public DaktelaSmsNotificationProvider(
            ILogger<DaktelaSmsNotificationProvider> logger,
            IOptionsSnapshot<CommonConfiguration> options)
        {
            this.logger = logger;
            this.options = options;
        }
        public async Task SendAsync(SmsNotification notification, string subject = default)
        {
            logger.LogInformation($"Sending sms notification for user : {notification.recipient}, with text : {notification.text}");

            Guard.NotNull(notification);
            Guard.NotNull(notification.recipient);
            Guard.StringNotNullOrWhiteSpace(notification.recipient.PhoneNumber);

            await SendMessage(notification.recipient.PhoneNumber!, notification.text);
        }

        private async Task SendMessage(string phoneNumber, string text)
        {
            Guard.StringNotNullOrWhiteSpace(text, nameof(text));

            ValidatePhoneNumber(phoneNumber);

            text = EncodeMessage(text);
                        
            try
            {
                var gate = options.Value?.SmsGate;

                ValidateGateConnection(gate);

                await UseHttpClient(async http => 
                {
                    var instance = gate.Address;
                    var accessToken = gate.AccessToken;
                    var queueId = gate.Queue;

                    var uri = $"{instance}?type=SMS&_method=POST&action=CLOSE&accessToken={accessToken}&number={phoneNumber}&queue={queueId}&text={text}";

                    var response = await http.GetAsync(uri);

                    if(!response.IsSuccessStatusCode)
                    {
                        var code = response.StatusCode;
                        var content = await response.Content.ReadAsStringAsync();

                        throw new InvalidOperationException($"Status code : {code}, content : {content}");
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occured when sending SMS using gate");

                var bypass = options.Value?.SmsGate?.ByPassFailure;

                if (!(bypass.HasValue && bypass.Value)) throw;
            }
        }

        private void ValidatePhoneNumber(string phoneNumber)
        {
            Guard.StringNotNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        }

        private string EncodeMessage(string text) => $"{text}";
        
        private void ValidateGateConnection(SmsGateConfiguration configuration)
        {
            Guard.NotNull(configuration, nameof(configuration));
            Guard.StringNotNullOrWhiteSpace(configuration.Address);
            Guard.StringNotNullOrWhiteSpace(configuration.AccessToken);
        }

        private async Task UseHttpClient(Func<HttpClient, Task> f)
        {
            using (var http = new HttpClient())
            {
                await f(http);
            }
        }
    }
}
