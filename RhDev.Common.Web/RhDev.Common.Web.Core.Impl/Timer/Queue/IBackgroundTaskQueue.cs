namespace RhDev.Common.Web.Core.Impl.Timer.Queue
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(QueueWorkItem workItem);

        QueueWorkItem Dequeue();

        IAsyncEnumerable<QueueWorkItem> DequeueAllAsync(CancellationToken cancellationToken);

        ValueTask<QueueWorkItem> DequeueAsync(
            CancellationToken cancellationToken);
    }
}
