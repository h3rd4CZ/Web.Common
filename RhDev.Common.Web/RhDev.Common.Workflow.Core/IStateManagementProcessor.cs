using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Management;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow
{
    public interface IStateManagementProcessor<in TParametrizededTriger> : IAutoregisteredService
    {
        Task FireTriggerAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters);

        Task<IList<WorkflowTransitionInfo>> GetAllPermittedTransitionAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters);

        Task<bool> IsInEndStateAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters);
        Task RolbackTransactionAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters);
        Task<TaskRespondStatus> CompleteTaskAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters);

    }
}
