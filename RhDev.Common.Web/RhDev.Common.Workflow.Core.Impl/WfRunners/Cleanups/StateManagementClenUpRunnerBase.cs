using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.WfRunners.Cleanups
{
    public abstract class StateManagementClenUpRunnerBase<TLogger> : IStateManagementCleanUpRunner
    {
        protected readonly ILogger<TLogger> traceLogger;

        public StateManagementClenUpRunnerBase(
            ILogger<TLogger> traceLogger)
        {
            this.traceLogger = traceLogger;
        }

        public async Task RunAsync()
        {
            var workerName = GetType().Name;

            traceLogger.LogTrace($"{workerName} started at {DateTime.Now}");

            try
            {
                await DoWork();
            }
            catch (Exception ex)
            {
                traceLogger.LogError(ex, $"An error occured when running {workerName}");
            }

            traceLogger.LogTrace($"{workerName} finished at {DateTime.Now}");
        }

        protected abstract Task DoWork();
    }
}
