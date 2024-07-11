using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Events;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow
{
    public interface IStateTransitionEvaluator : IAutoregisteredService
    {
        Task<bool> EvaluateTransitionConditionAsync(bool evaluatePermission, StateTransitionEventArgs args);
        void EvaluateUserProperties(Transition transition, StateManagementCommonTriggerProperties userData);
    }
}
