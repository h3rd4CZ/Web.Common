using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Impl.DependencyInjection;
using RhDev.Common.Web.Core.Impl.Timer.Cron;
using RhDev.Common.Web.Core.Impl.Timer.Queue;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Timer
{
    public static class HostedServiceExtensions
    {
        private const string QUEUE_SERVICE_CAPACITY_DEFAULT = "1000";

        public static IServiceCollection AddQueueHostedService(this IServiceCollection services, IConfiguration configuration, CommonWebRegistrationBuilder commonBuilder)
        {
                                    
            var queueCapacity = commonBuilder.QueueHostedServiceCapacity.HasValue 
                ? commonBuilder.QueueHostedServiceCapacity.Value.ToString() 
                : configuration.GetSection(ConfigurationUtils.GetPathConfigurationProperty<CommonConfiguration>(a => a.QueueService.QueueCapacity))?.Value;

            queueCapacity ??= QUEUE_SERVICE_CAPACITY_DEFAULT;
            
            if (!int.TryParse(queueCapacity, out int capacity)) throw new InvalidOperationException("Queue capacity is not a number");                      
            
            services.AddSingleton<IBackgroundTaskQueue>(ctx => new BackgroundTaskQueue(capacity));

            services.AddSingleton(commonBuilder);

            services.AddHostedService<QueuedHostedService>();
                
            return services;
        }

        public static IServiceCollection NewHostedService<T>(this IServiceCollection services) where T : class, IHostedService
        {
            services.AddHostedService<T>();

            return services;
        }

        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : CronJobService<T>
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), @"Please provide Schedule Configurations.");
            }
            var config = new ScheduleConfig<T>();

            options.Invoke(config);

            if (string.IsNullOrWhiteSpace(config.CronExpression))
            {
                throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), @"Empty Cron Expression is not allowed.");
            }

            services.AddSingleton<IScheduleConfig<T>>(config);

            services.AddHostedService<T>();

            return services;
        }
    }
}
