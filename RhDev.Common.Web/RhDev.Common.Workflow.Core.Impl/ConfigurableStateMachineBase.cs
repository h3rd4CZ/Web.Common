using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.StateDefinition;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Impl.Exceptions;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RhDev.Common.Workflow.Impl
{
    public abstract class ConfigurableStateMachineBase
    {
        public const string SHAREPOINT_UPDATE_HANDLER = "UpdateEventHandler";

        private const string TRIGGERCODE_SCHEMA_WITH_ORDER = "TO{0}FROM{1}${2}";
        protected readonly StateMachineRuntimeParameters stateMachineRuntimeParameters;
        private readonly IWorkflowTransitionLogRepository workflowTransitionLogRepository;
        private readonly IStateTransitionEventHandler[] stateTransitionEventHandlers;
        protected SectionDesignation designation;

        private readonly IStateTransitionExternalLogging _stateTransitionExternalLogging;
        private readonly ILogger<ConfigurableStateMachineBase> logger;
        private IStateTransitionEventHandler handler;

        protected readonly IDictionary<string, StateMachine<string, string>.TriggerWithParameters<StateManagementCommonTriggerProperties>> _parametrizedTriggers;
        protected readonly IDictionary<string, Configuration.StateMachineConfig.Transitions.Transition> _parametrizedTriggersTransitions;

        protected IList<string> endStates;
        protected IList<string> systemStates = new List<string>();
        protected StateMachine<string, string> _machine;
        protected IDictionary<string, StateDefinition> stateDefinitions;
                        
        protected bool detectedApprovalSkiping;
        protected StateTransitionEventArgs stateTransitionEventArgs;

        public void SetApprovalSkippingDetected() => detectedApprovalSkiping = true;

        protected ConfigurableStateMachineBase(
            StateMachineRuntimeParameters stateMachineRuntimeParameters,
            IWorkflowTransitionLogRepository workflowTransitionLogRepository,
            IStateTransitionEventHandler[] stateTransitionEventHandlers,
            IStateTransitionExternalLogging stateTransitionExternalLogging,
            ILogger<ConfigurableStateMachineBase> logger
            )
        {
            this.stateMachineRuntimeParameters = stateMachineRuntimeParameters;
            this.workflowTransitionLogRepository = workflowTransitionLogRepository;
            this.stateTransitionEventHandlers = stateTransitionEventHandlers;

            if (!Equals(null, stateTransitionEventHandlers))
                this.stateTransitionEventHandlers = this.stateTransitionEventHandlers.OrderBy(h => h.EvaluationOrder).ToArray();

            designation = stateMachineRuntimeParameters.DocumentMetadataIdentifier.SectionDesignation;
            _stateTransitionExternalLogging = stateTransitionExternalLogging;
            this.logger = logger;

            _parametrizedTriggers = new Dictionary<string, StateMachine<string, string>.TriggerWithParameters<StateManagementCommonTriggerProperties>>();
            _parametrizedTriggersTransitions = new Dictionary<string, Configuration.StateMachineConfig.Transitions.Transition>();

            endStates = new List<string>();

            detectedApprovalSkiping = false;

            stateDefinitions = new Dictionary<string, StateDefinition>();
        }

        /// <summary>
        /// Lookup for state with O(1)
        /// </summary>
        /// <param name="code">State code</param>
        /// <returns></returns>
        public StateDefinition GetStateDefinitionByCode(string code)
        {
            StateDefinition sd;
            if (!stateDefinitions.TryGetValue(code, out sd))
                throw new InvalidOperationException($"State with code : {code} doesnt exist");

            return sd;
        }

        /// <summary>
        /// Lookup for state with O(n)
        /// </summary>
        /// <param name="title">Title of a state</param>
        /// <returns></returns>
        public StateDefinition GetStateDefinitionByTitle(string title)
        {
            var state = stateDefinitions.FirstOrDefault(s => s.Value.Title.Equals(title));

            if (Equals(null, state)) throw new InvalidOperationException($"State with title {title} not found");

            return state.Value;
        }

        /// <summary>
        /// check whether the state is start state
        /// </summary>
        /// <param name="stateTitle">Title of a state</param>
        /// <returns></returns>
        public bool IsStateStart(string stateTitle)
        {
            if (string.IsNullOrEmpty(stateTitle)) throw new ArgumentNullException(nameof(stateTitle));

            var stateDef = GetStateDefinitionByTitle(stateTitle);

            return stateDef.IsStart;
        }

        public bool IsStateEnd(string stateTitle)
        {
            if (string.IsNullOrEmpty(stateTitle)) throw new ArgumentNullException(nameof(stateTitle));

            var stateDef = GetStateDefinitionByTitle(stateTitle);

            return stateDef.IsEnd;
        }

        /// <summary>
        /// Check if state is system
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsStateSystem(string state)
        {
            Guard.StringNotNullOrWhiteSpace(state);

            return systemStates.Any(s => s == state);
        }


        protected void AddEndState(string state)
        {
            endStates.Add(state);
        }

        protected void AddParametrizedTrigger(string triggerCode,
            StateMachine<string, string>.TriggerWithParameters<StateManagementCommonTriggerProperties> paramTrigger)
        {
            if (string.IsNullOrEmpty(triggerCode)) throw new ArgumentNullException(nameof(triggerCode));
            if (paramTrigger == null) throw new ArgumentNullException(nameof(paramTrigger));

            if (_parametrizedTriggers.ContainsKey(triggerCode)) throw new InvalidOperationException($"Trigger {triggerCode} has been already defined in parametrized triggers");

            _parametrizedTriggers.Add(triggerCode, paramTrigger);
        }

        protected Configuration.StateMachineConfig.Transitions.Transition GetParametrizedTransition(string triggerCode)
        {
            if (string.IsNullOrEmpty(triggerCode)) throw new ArgumentNullException(nameof(triggerCode));

            Configuration.StateMachineConfig.Transitions.Transition t;
            if (_parametrizedTriggersTransitions.TryGetValue(triggerCode, out t)) return t;

            throw new InvalidOperationException($"Parametrized transition for trigger {triggerCode} was not found");
        }

        protected void AddTriggerTransition(string triggerCode,
            Configuration.StateMachineConfig.Transitions.Transition transition)
        {
            if (string.IsNullOrEmpty(triggerCode)) throw new ArgumentNullException(nameof(triggerCode));
            if (transition == null) throw new ArgumentNullException(nameof(transition));

            if (_parametrizedTriggersTransitions.ContainsKey(triggerCode)) throw new InvalidOperationException($"Trigger {triggerCode} has been already defined in trigger transitions");

            _parametrizedTriggersTransitions.Add(triggerCode, transition);
        }

        protected async Task Rollback(string triggerCode)
        {
            IList<WorkflowTransitionLog> transactionData = new List<WorkflowTransitionLog>();

            var transactionTransition = GetParametrizedTransition(triggerCode);
                        
            transactionData =
                await workflowTransitionLogRepository.ReadAsync(
                    t => t.TransitionId == stateMachineRuntimeParameters.UserData.TransitionId.ToString(),
                    include : new List<Expression<Func<WorkflowTransitionLog, object>>> 
                    {
                        l => l.WorkflowInstance, 
                        l => l.WorkflowInstance.WorkflowDocument });

            var failTransactionItem = transactionData.FirstOrDefault(t => !string.IsNullOrEmpty(t.Result));

            if (Equals(null, failTransactionItem))
            {
                //throw new InvalidOperationException(
                //    $"Fail handler for transaction {_userData.TransitionId} was not found");

                //DO NOTHING because we dont have fail handler an error did not happen
                //and can repeat transaction
                return;
            }

            var failHandlerClass =
                stateTransitionEventHandlers.FirstOrDefault(c => c.GetType().Name.Equals(failTransactionItem.Handler, StringComparison.OrdinalIgnoreCase));

            if (Equals(null, failHandlerClass))
            {
                //Fail handler is Sharepoint update which is not implemented as class only code in Statemanagementprocessor
                //Rollback must start from last handler in transaction chain
                failHandlerClass = stateTransitionEventHandlers[stateTransitionEventHandlers.Length - 1];
            }

            var failhandlerClassChainIndex = stateTransitionEventHandlers
                .ToList()
                .IndexOf(failHandlerClass);

            for (int i = failhandlerClassChainIndex; i >= 0; i--)
            {
                var currenthandlerClass = stateTransitionEventHandlers[i];

                var currentHandlerClassName = currenthandlerClass.GetType().Name;

                var transactionChainData =
                    transactionData.FirstOrDefault(t => t.Handler.Equals(currentHandlerClassName, StringComparison.OrdinalIgnoreCase));

                if (Equals(null, transactionChainData)) continue;

                stateTransitionEventHandlers[i].RehydrateState(transactionChainData.UserData);

                await stateTransitionEventHandlers[i].RollbackAsync(
                    this, 
                    stateMachineRuntimeParameters.DocumentMetadataIdentifier.SectionDesignation, 
                    stateMachineRuntimeParameters.UserData, 
                    stateMachineRuntimeParameters.DocumentMetadataIdentifier, 
                    transactionTransition, transactionChainData);
            }
        }

        protected async Task DoTransition(StateManagementCommonTriggerProperties tranProps,
            StateMachine<string, string>.Transition tran)
        {
            Configuration.StateMachineConfig.Transitions.Transition configuredTransition = GetParametrizedTransition(tran.Trigger);

            stateTransitionEventArgs = new StateTransitionEventArgs(stateMachineRuntimeParameters.DocumentMetadataIdentifier.SectionDesignation, tranProps, tran.Source,
                tran.Destination, configuredTransition,
                stateMachineRuntimeParameters.DocumentMetadataIdentifier,
                (int)stateMachineRuntimeParameters.Workflow.DataId, stateMachineRuntimeParameters.UserId, stateMachineRuntimeParameters.UserPermissionGroups, stateMachineRuntimeParameters.EvaluateMembershipLocally);

            foreach (var @event in stateTransitionEventHandlers)
            {
                await RunEvaluationHandler(@event, stateTransitionEventArgs);

                if (detectedApprovalSkiping)
                {
                    designation = stateTransitionEventArgs.Designation;
                    return;
                }
            }

            designation = stateTransitionEventArgs.Designation;
        }

        protected string GetTriggerCode(string from, string to, string order)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            return string.Format(TRIGGERCODE_SCHEMA_WITH_ORDER, to, from, string.IsNullOrEmpty(order) ? "0" : order);
        }
            
        private async Task RunEvaluationHandler(IStateTransitionEventHandler handler, StateTransitionEventArgs @event)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            string handlerClass = handler.GetType().Name;

            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;

            try
            {
                start = DateTime.Now;

                await handler.OnTransitionAsync(this, @event);

                end = DateTime.Now;

                await _stateTransitionExternalLogging.LogAsync(handlerClass, default!, @event, start, end, handler.HandlerState);
            }
            catch (Exception ex)
            {
                end = DateTime.Now;

                await _stateTransitionExternalLogging.LogAsync(handlerClass, ex, @event, start, end, handler.HandlerState);

                throw;
            }
        }
    }

}
