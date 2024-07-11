using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.DependencyInjection
{
    public class CommonWebRegistrationBuilder
    {
        public bool QueueHostedService { get; private set; }
        public int? QueueHostedServiceWorkers { get; private set; }
        public int? QueueHostedServiceCapacity { get; private set; }

        public void UseQueueHostedService() => QueueHostedService = true;

        public void UseQueueHostedService(int workerCount, int queueCapacity)
        {
            Guard.NumberMin(workerCount, 1);
            Guard.NumberMin(queueCapacity, 1);

            QueueHostedService = true;

            QueueHostedServiceCapacity = queueCapacity;

            QueueHostedServiceWorkers = workerCount;
        }
    }
}
