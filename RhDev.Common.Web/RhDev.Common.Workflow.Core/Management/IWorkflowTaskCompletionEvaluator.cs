using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.StateDefinition;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Management
{
    public interface IWorkflowTaskCompletionEvaluator : IAutoregisteredService
    {
        Task<TaskRespondStatus> CompleteTaskAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters, StateMachine sm, StateDefinition currentStateDefinition, ConfiguredState configuredState, Transition transition);
    }
}
