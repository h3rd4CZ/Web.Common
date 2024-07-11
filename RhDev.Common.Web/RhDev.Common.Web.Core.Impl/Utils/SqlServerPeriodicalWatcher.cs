using Microsoft.Extensions.Primitives;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Utils
{
    internal class SqlServerPeriodicalWatcher : ISqlServerWatcher
    {
        private readonly TimeSpan _refreshInterval;
        private IChangeToken _changeToken;
        private readonly System.Threading.Timer _timer;
        private CancellationTokenSource _cancellationTokenSource;

        public SqlServerPeriodicalWatcher(TimeSpan refreshInterval)
        {
            _refreshInterval = refreshInterval;

            _timer = new System.Threading.Timer(Change, null, TimeSpan.FromSeconds(15), _refreshInterval);
        }

        private void Change(object state)
        {
            _cancellationTokenSource?.Cancel();
        }

        public IChangeToken Watch()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

            return _changeToken;
        }

        public void Dispose()
        {
            _timer?.Dispose();

            _cancellationTokenSource?.Dispose();
        }
    }
}
