using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Actions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Actions
{
    public class StateManagementConfiguredActionManager : IStateManagementConfiguredActionManager
    {
        public const string ActionFaultKey = "__Fault";
        private readonly IServiceProvider serviceProvider;
        private readonly IWorkflowPropertyManager workflowPropertyManager;
        private readonly IConditionEvaluator conditionEvaluator;

        public StateManagementConfiguredActionManager(
            IServiceProvider serviceProvider,
            IWorkflowPropertyManager workflowPropertyManager,
            IConditionEvaluator conditionEvaluator)
        {
            this.serviceProvider = serviceProvider;
            this.workflowPropertyManager = workflowPropertyManager;
            this.conditionEvaluator = conditionEvaluator;
        }

        public IList<StateManagementCompletedAction> CompletedActions { get; } = new List<StateManagementCompletedAction> { };

        public async Task RunAsync(StateTransitionEventArgs transitionArgs, List<StateMachineConfiguredAction> actions)
        {
            CheckConfiguredActions(actions);

            foreach (StateMachineConfiguredAction configuredAction in actions)
            {
                if (configuredAction.Disabled || !await ActionCondition(transitionArgs, configuredAction)) continue;

                IStateManagementConfiguredAction actionImpl = default!;
                string actionIdentifier = string.Empty;
                Exception? actionFault = default!;
                try
                {
                    actionImpl = GetActionImplementation(configuredAction);
                    actionIdentifier = configuredAction.Id;
                    try
                    {
                        await actionImpl.ExecuteAsync(transitionArgs, configuredAction.Parameters);
                    }
                    catch (Exception ex)
                    {
                        if (configuredAction.SuppressException) actionFault = ex;
                        else throw;
                    }
                }
                finally
                {
                    if (!string.IsNullOrEmpty(actionIdentifier))
                    {
                        var completedActionState = new StateManagementCompletedAction()
                        {
                            Identifier = actionIdentifier
                        };

                        foreach (var state in actionImpl.ActionState) completedActionState.Params[state.Key] = state.Value;
                        if (actionFault is not null) completedActionState.Params[ActionFaultKey] = actionFault.ToString();

                        CompletedActions.Add(completedActionState);
                    }
                }
            }
        }

        private async Task<bool> ActionCondition(StateTransitionEventArgs transitionArgs, StateMachineConfiguredAction configuredAction)
        {
            Guard.NotNull(transitionArgs, nameof(transitionArgs));
            Guard.NotNull(configuredAction, nameof(configuredAction));

            if (Equals(null, configuredAction.Condition)) return true;

            return await conditionEvaluator.EvaluateAsync(transitionArgs, configuredAction.Condition);
        }

        public async Task RollbackAsync(IDictionary<string, string> actionParams, SectionDesignation designation, List<StateMachineConfiguredAction> transitionActions)
        {
            if (actionParams == null) throw new ArgumentNullException(nameof(actionParams));
            if (designation == null) throw new ArgumentNullException(nameof(designation));

            CheckConfiguredActions(transitionActions);

            var reversedActions = actionParams.Keys.Reverse();

            foreach (string actionKey in reversedActions)
            {
                var actionIdentifier = actionKey;
                var actionRollbackParams = actionParams[actionKey];

                var ai = GetActionImplementation(actionIdentifier, transitionActions);

                await ai.RollbackAsync(designation, actionRollbackParams);
            }
        }
                
        private IStateManagementConfiguredAction GetActionImplementation(string actionIdentifier, List<StateMachineConfiguredAction> transitionActions)
        {
            Guard.StringNotNullOrWhiteSpace(actionIdentifier, nameof(actionIdentifier));

            var configuredAction = transitionActions.FirstOrDefault(a => a.Id == actionIdentifier);

            Guard.NotNull(configuredAction, nameof(configuredAction), $"Action definition with identifier : {actionIdentifier} was not found in transition action list");

            return GetActionImplementation(configuredAction);
        }

        private IStateManagementConfiguredAction GetActionImplementation(StateMachineConfiguredAction configuredAction)
        {
            Guard.NotNull(configuredAction, nameof(configuredAction));
            Guard.StringNotNullOrWhiteSpace(configuredAction.TypeName, nameof(configuredAction.TypeName));
                        
            var actionType = Type.GetType(configuredAction.TypeName);

            Guard.NotNull(actionType, nameof(actionType), $"Action Type : {configuredAction.TypeName} was not found");

            CheckActionType(actionType);

            var service = serviceProvider.GetRequiredService(actionType);

            var typedService = (IStateManagementConfiguredAction)service;
                        
            return typedService;
        }

        private void CheckActionType(Type actionType)
        {
            if (!typeof(IStateManagementConfiguredAction).IsAssignableFrom(actionType))
                throw new InvalidOperationException($"Action type {actionType.Name} is not {typeof(IStateManagementConfiguredAction).Name}");
        }
                
        private void CheckConfiguredActions(List<StateMachineConfiguredAction> actions)
        {
            Guard.NotNull(actions, nameof(actions));

            if (actions.Any(a => string.IsNullOrWhiteSpace(a.Id))) throw new InvalidOperationException($"There is at least one action with invalid ID, please ensure that the ID attribute has valid ID");

            if (actions.Any(a => string.IsNullOrWhiteSpace(a.TypeName))) throw new InvalidOperationException($"There is at least one action with invalid TypeName, please ensure that the TypeName has valid type definition");

            if (actions.Distinct().Count() != actions.Count) throw new InvalidOperationException("There is at least one action with same ID, please ensure that the ID attribute is unique accross one transition configuration");
        }
    }

}
