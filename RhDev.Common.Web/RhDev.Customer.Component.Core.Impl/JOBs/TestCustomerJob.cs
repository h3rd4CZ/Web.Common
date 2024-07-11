using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.Impl.Timer.Cron;

namespace RhDev.Customer.Component.Core.Impl.JOBs
{
    public class TestCustomerJob : CronJobService<TestCustomerJob>
    {
        protected override LogLevel JobWorkingLogLevel => LogLevel.Trace;
        public TestCustomerJob(
            IScheduleConfig<TestCustomerJob> config,
            ILogger<TestCustomerJob> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
            : base(config.CronExpression, config.RunOnStart, config.TimeZoneInfo, serviceProvider, logger, configuration) { }


        protected override async Task DoWork(CancellationToken cancellationToken)
        {
            await UseConfigurationRefreshedService<IFoo>(async svc =>
            {
                svc.Boo();
                
                logger.LogInformation($"Now : {DateTime.Now}");

                throw new InvalidOperationException("An error occured");
            });
        }
    }
}
