using Medallion.Threading.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Management;

namespace RhDev.Common.Workflow.DataAccess.SharePoint.Online.Management
{
    public class SafeLock : ISafeLock
    {
        private const string DOCUMENT_LOCK_FORMAT = "WORKFLOW_DOCUMENT_STORE_{0}";
        private const string TimeLogFormat = "MM/dd/yyyy hh:mm:ss.fff";
        private readonly IServiceProvider serviceProvider;
        private readonly IOptionsSnapshot<CommonConfiguration> globalOptions;
        private readonly ILogger<SafeLock> logger;
        private string _connString;
        private TimeSpan _lockTimeout;


        public SafeLock(
            IServiceProvider serviceProvider,
            IOptionsSnapshot<CommonConfiguration> globalOptions,
            ILogger<SafeLock> logger)
        {
            this.serviceProvider = serviceProvider;
            this.globalOptions = globalOptions;
            this.logger = logger;
        }

        public async Task UseDistributedLockForWorkflowDocumentAsync(StateMachineRuntimeParameters runtimeParameters, Func<Task> actionUnderLock, string methodName)
        {
            ValidateRuntimeParametersForDocumentLocking(runtimeParameters);

            var lockIdentifier = GetLockIdenitficator(runtimeParameters.DocumentMetadataIdentifier);

            var @lock = GetLock(lockIdentifier);

            using (@lock.Acquire(GetLockTimeout()))
            {
                logger.LogTrace($"Executing thread {Thread.CurrentThread.ManagedThreadId} acquired document lock : {lockIdentifier} for method : {methodName} at : {DateTime.Now.ToString(TimeLogFormat)}");

                await actionUnderLock();

                logger.LogTrace($"Executing thread {Thread.CurrentThread.ManagedThreadId} released document lock {lockIdentifier} for method : {methodName} at : {DateTime.Now.ToString(TimeLogFormat)}");
            }
        }

        public async Task<TReturn> UseDistributedLockForWorkflowDocumentAndReturnAsync<TReturn>(StateMachineRuntimeParameters runtimeParameters, Func<Task<TReturn>> funcUnderLock, string methodName)
        {
            ValidateRuntimeParametersForDocumentLocking(runtimeParameters);

            var lockIdentifier = GetLockIdenitficator(runtimeParameters.DocumentMetadataIdentifier);

            var @lock = GetLock(lockIdentifier);

            using (@lock.Acquire(GetLockTimeout()))
            {
                logger.LogTrace($"Executing thread {Thread.CurrentThread.ManagedThreadId} acquired document lock {lockIdentifier} for method : {methodName} at : {DateTime.Now.ToString(TimeLogFormat)}");

                var result = await funcUnderLock();

                logger.LogTrace($"Executing thread {Thread.CurrentThread.ManagedThreadId} released document lock {lockIdentifier} for method : {methodName} at : {DateTime.Now.ToString(TimeLogFormat)}");

                return result;
            }
        }

        private string GetLockIdenitficator(WorkflowDocumentIdentifier identifier)
        {
            var id = $"{identifier.Identificator.typeName}_{identifier.Identificator.entityId}";

            return string.Format(DOCUMENT_LOCK_FORMAT, id);
        }

        private void ValidateRuntimeParametersForDocumentLocking(StateMachineRuntimeParameters runtimeParameters)
        {
            Guard.NotNull(runtimeParameters, nameof(runtimeParameters));
            Guard.NotNull(runtimeParameters.DocumentMetadataIdentifier, nameof(runtimeParameters.DocumentMetadataIdentifier), $"There are no valid document metadata for distributed lock pattern");
        }

        private SqlDistributedLock GetLock(string lockName)
        {
            Guard.StringNotNullOrWhiteSpace(lockName, nameof(lockName));

            var connString = GetConnectionString();

            Guard.NotNull(connString, nameof(connString));

            return new SqlDistributedLock(lockName, connString);
        }

        private string GetConnectionString()
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(_connString))
                {
                    lock (this)
                    {
                        var dbCtx = serviceProvider.GetService<DbContext>();

                        Guard.NotNull(dbCtx, nameof(dbCtx), $"There is no DbContext registered within DI container");

                        var ciInfo = dbCtx.Database?.GetConnectionString();

                        Guard.StringNotNullOrWhiteSpace(ciInfo!);

                        _connString = ciInfo;
                        return _connString;
                    }
                }

                return _connString;
            }
        }

        private TimeSpan GetLockTimeout()
        {
            lock (this)
            {
                if (Equals(default(TimeSpan), _lockTimeout))
                {
                    lock (this)
                    {
                        var dlTimeout = globalOptions.Value.Workflow.DistributedLockTimeoutInSeconds;
                                                
                        var span = TimeSpan.FromSeconds(dlTimeout);

                        _lockTimeout = span;

                        return span;
                    }
                }

                return _lockTimeout;
            }
        }
    }
}
