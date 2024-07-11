using System.Threading.Channels;

namespace RhDev.Common.Web.Core.Impl.Timer.Queue
{
    public record QueueWorkItem(string id, Func<CancellationToken, ValueTask> worker);
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<QueueWorkItem> _queue;

        public BackgroundTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };

            _queue = Channel.CreateBounded<QueueWorkItem>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(
            QueueWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _queue.Writer.WriteAsync(workItem);
        }

        public QueueWorkItem Dequeue()
        {
            if (_queue.Reader.TryRead(out QueueWorkItem workItem)) return workItem;

            return default;
        }

        public async ValueTask<QueueWorkItem> DequeueAsync(
            CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }

        public IAsyncEnumerable<QueueWorkItem> DequeueAllAsync(
            CancellationToken cancellationToken)
        {
            return _queue.Reader.ReadAllAsync(cancellationToken);

        }
    }
}
