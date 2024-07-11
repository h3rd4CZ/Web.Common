using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Actions
{
    public class StateManagementCompletedAction
    {
        public string Identifier { get; set; }
        public Dictionary<string, object> Params { get; set; } = new();
    }

    public interface IStateManagementConfiguredActionManager : IAutoregisteredService
    {
        IList<StateManagementCompletedAction> CompletedActions { get; }
        Task RunAsync(StateTransitionEventArgs transitionArgs, List<StateMachineConfiguredAction> actions);

        Task RollbackAsync(IDictionary<string, string> actionParams, SectionDesignation designation, List<StateMachineConfiguredAction> transitionActions);
    }
}
