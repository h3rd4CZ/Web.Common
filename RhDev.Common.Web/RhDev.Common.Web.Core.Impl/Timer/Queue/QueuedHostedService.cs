using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Impl.DependencyInjection;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Timer.Queue
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly CommonWebRegistrationBuilder commonWeb;
        private readonly IOptions<CommonConfiguration> options;
        private readonly ILogger<QueuedHostedService> _logger;
                
        public QueuedHostedService(
            CommonWebRegistrationBuilder commonWeb,
            IOptions<CommonConfiguration> options,
            IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger)
        {
            this.commonWeb = commonWeb;
            this.options = options;
            TaskQueue = taskQueue;
            _logger = logger;
        }

        public IBackgroundTaskQueue TaskQueue { get; }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogTrace($"Queued Hosted Service started");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task WorkItemWorker(CancellationToken cancellationToken, int workerNr)
        {
            var workerId = Thread.CurrentThread.ManagedThreadId;

            _logger.LogTrace($"Worker nr : {workerNr} started at thread : {workerId}");

            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem =
                    TaskQueue.Dequeue();

                if (workItem is null)
                {
                    Thread.Sleep(500);

                    continue;
                }

                _logger.LogInformation($"Queue hosted service DEQUEUED TASK {workItem.id} at worker : {workerId} at :{DateTime.Now}");

                try
                {
                    await workItem.worker(cancellationToken);

                    _logger.LogInformation($"Queue hosted service processed QUEUED TASK {workItem.id} at worker : {workerId} at :{DateTime.Now}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        $"Queue hosted task error occurred at {workItem.id}. at worker : {workerId} at {DateTime.Now}", nameof(workItem));
                }
            }

            _logger.LogTrace($"Worker nr : {workerNr} thread id : {workerId} stopped...");
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            var threads = commonWeb.QueueHostedServiceWorkers.HasValue
                ? commonWeb.QueueHostedServiceWorkers.Value
                : options.Value.QueueService.Workers;

            Guard.NumberMin(threads, 1, nameof(threads));

            foreach (var i in Enumerable.Range(1, threads))
            {
                var thread = new Thread(async o => { await WorkItemWorker((CancellationToken)o, i); });

                thread.Start(stoppingToken);
            }
        }


        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogTrace("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
