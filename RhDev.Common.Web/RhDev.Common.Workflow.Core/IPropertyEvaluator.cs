using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Workflow
{
    public interface IPropertyEvaluator : IAutoregisteredService
    {
        Task<StateManagementValue> EvaluateAsync(Operand property, StateTransitionEventArgs args);
    }
}
