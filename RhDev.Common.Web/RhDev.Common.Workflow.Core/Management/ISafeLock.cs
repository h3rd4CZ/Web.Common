using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using System;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Management
{
    public interface ISafeLock : IAutoregisteredService
    {
        Task UseDistributedLockForWorkflowDocumentAsync(StateMachineRuntimeParameters runtimeParameters, Func<Task> actionUnderLock, string methodName);
        Task<TReturn> UseDistributedLockForWorkflowDocumentAndReturnAsync<TReturn>(StateMachineRuntimeParameters runtimeParameters, Func<Task<TReturn>> funcUnderLock, string methodName);
    }
}
