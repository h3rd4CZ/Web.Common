using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Actions
{
    public interface IStateManagementConfiguredAction
    {
        Task ExecuteAsync(StateTransitionEventArgs args, List<StateMachineActionParameter> parameters);
        IDictionary<string, string> ActionState { get; }
        Task RollbackAsync(SectionDesignation designation, string @params);
    }
}
