using Cronos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace RhDev.Common.Web.Core.Impl.Timer.Cron
{
    public abstract class CronJobService<TCronJobType> : IHostedService, IDisposable where TCronJobType : class
    {
        private System.Timers.Timer? _timer;
        protected readonly CronExpression _expression;
        private readonly bool runOnStart;
        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly IServiceProvider serviceProvider;
        protected readonly ILogger<TCronJobType> logger;
        private readonly IConfiguration configuration;

        protected virtual LogLevel JobWorkingLogLevel => LogLevel.Information;

        protected CronJobService(
            string cronExpression,
            bool runOnStart,
            TimeZoneInfo timeZoneInfo, IServiceProvider serviceProvider, ILogger<TCronJobType> logger, IConfiguration configuration)
        {
            _expression = CronExpression.Parse(cronExpression);
            this.runOnStart = runOnStart;
            _timeZoneInfo = timeZoneInfo;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.configuration = configuration;
        }

        protected T CreateService<T>() where T : notnull
        {
            using (var scope = serviceProvider.CreateScope()) return scope.ServiceProvider.GetRequiredService<T>();
        }

        protected async Task UseConfigurationRefreshedService<T>(Func<T, Task> a) where T : notnull
        {
            using (var scope = serviceProvider.CreateScope())
            {
                await UseConfigurationRefreshedServiceWithNoScopedContainer<T>(a, scope.ServiceProvider);
            }
        }

        protected async Task UseConfigurationRefreshedServiceWithNoScopedContainer<T>(Func<T, Task> a, IServiceProvider provider = default!) where T : notnull
        {
            provider ??= serviceProvider;

            var config = provider.GetRequiredService<IConfiguration>();

            RefreshConfig(config);

            var svc = provider.GetRequiredService<T>();

            await a(svc);
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            LogInitializing();

            if (runOnStart) await DoWorkInternal(cancellationToken);

            await ScheduleJob(cancellationToken);
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);

            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds <= 0)
                {
                    await ScheduleJob(cancellationToken);
                }
                _timer = new System.Timers.Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Dispose();
                    _timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        logger.Log(JobWorkingLogLevel, $"{typeof(TCronJobType).Name} CRON JOB is working.");

                        await DoWorkInternal(cancellationToken);

                        logger.Log(JobWorkingLogLevel, $"{typeof(TCronJobType).Name} CRON JOB finished working.");
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ScheduleJob(cancellationToken);
                    }
                };
                _timer.Start();
            }
            await Task.CompletedTask;
        }

        protected abstract Task DoWork(CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogTrace($"{typeof(TCronJobType).Name} CRON JOB is stopping.");

            _timer?.Stop();

            await Task.CompletedTask;
        }

        private void LogInitializing()
        {
            var nextOccurencies = _expression.GetOccurrences(DateTime.Now.ToUniversalTime(), DateTime.Now.ToUniversalTime().AddDays(1));

            nextOccurencies = nextOccurencies.Take(5);

            logger.LogTrace($"Next occurencies of JOB : ");

            foreach (var occurence in nextOccurencies) logger.LogTrace($"{occurence}");
        }

        private async Task DoWorkInternal(CancellationToken cancellationToken)
        {
            try
            {
                RefreshConfig();

                await DoWork(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"{typeof(TCronJobType).Name} CRON JOB failed : {ex}");
            }
        }

        private void RefreshConfig(IConfiguration config = default)
        {
            ((IConfigurationRoot)(config ?? configuration)).Reload();
        }
    }
}
