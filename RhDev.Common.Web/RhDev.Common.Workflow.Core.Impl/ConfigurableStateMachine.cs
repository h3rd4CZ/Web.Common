using Microsoft.Extensions.Logging;
using RhDev.Common.Workflow.Configuration.StateMachineConfig;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.StateDefinition;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Management;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Impl
{
    public class ConfigurableStateMachine : ConfigurableStateMachineBase, IConfigurableStateMachine<string, StateManagementCommonTriggerProperties>
    {
        private const string STATE_REF_SOURCE_PLACEHOLDER = "{SOURCE}";
        private readonly WorkflowInstance workflowInstance;
        private readonly IStateMachineConfigurationProvider _configurationProvider;
        
        private readonly IStateTransitionEvaluator _transitionEvaluator;
        private readonly IWorkflowTaskCompletionEvaluator workflowTaskCompletitionEvaluator;

        public bool IsInEndState => endStates.Contains(_machine.State);
        public StateManagementCommonTriggerProperties UserData => stateMachineRuntimeParameters.UserData;
        public StateTransitionEventArgs StateTransitionEventArgs => stateTransitionEventArgs;
        public WorkflowInstance WorkflowInstance => workflowInstance;
        public SectionDesignation Designation => designation;
                
        /// <param name="documentMetadata"></param>
        /// <param name="sectionDesignation"></param>
        /// <param name="userLogin"></param>
        /// <param name="userGroups"></param>
        /// <param name="evaluateTasks"></param>
        /// <param name="evaluatePermission"></param>
        /// <param name="evaluateSystemTriggers"></param>
        /// <param name="configurationProvider"></param>
        /// <param name="groupInfoProvider"></param>
        /// <param name="userInfoprovider"></param>
        /// <param name="documentHistory"></param>
        /// <param name="transitionEvaluator"></param>
        /// <param name="stateTransitionExternalLogging"></param>
        public ConfigurableStateMachine(
            StateMachineRuntimeParameters stateMachineRuntimeParameters,
            WorkflowInstance workflowInstance,
            IStateMachineConfigurationProvider configurationProvider,
            IStateTransitionEvaluator transitionEvaluator,
            IStateTransitionExternalLogging stateTransitionExternalLogging,
            IStateTransitionEventHandler[] stateTransitionEventHandlers,
            IWorkflowTransitionLogRepository workflowTransitionLogRepository,
            ILogger<ConfigurableStateMachineBase> traceLogger,
            IWorkflowTaskCompletionEvaluator workflowTaskCompletitionEvaluator
            ) : base(stateMachineRuntimeParameters, workflowTransitionLogRepository, stateTransitionEventHandlers, stateTransitionExternalLogging, traceLogger)
        {
            this.workflowInstance = workflowInstance;
            ValidateRuntimeParameters(stateMachineRuntimeParameters);
            Guard.NotNull(workflowInstance, nameof(workflowInstance));
            _configurationProvider = configurationProvider;
            _transitionEvaluator = transitionEvaluator;
            this.workflowTaskCompletitionEvaluator = workflowTaskCompletitionEvaluator;
        }
                
        public bool ApprovalSkippingDetected => detectedApprovalSkiping;

        public async Task<IList<Configuration.StateMachineConfig.Transitions.Transition>> GetAllPermittedTransitionsAsync()
        {
            await Init(stateMachineRuntimeParameters.EvaluateSystemTriggers, stateMachineRuntimeParameters.Workflow, true);

            var permittedTriggers = _machine.PermittedTriggers;
                        
            var triggers = permittedTriggers.Select(
                p =>
                {
                    Configuration.StateMachineConfig.Transitions.Transition transition;
                    if (!_parametrizedTriggersTransitions.TryGetValue(p, out transition))
                        throw new InvalidOperationException($"Trigger settings for trigger : {p} was not found");
                    return transition;
                }).ToList();
            
            return triggers;
        }
        
        public async Task FireParametrizedAsync(string trigger, StateManagementCommonTriggerProperties props)
        {
            if (string.IsNullOrEmpty(trigger)) throw new ArgumentNullException(nameof(trigger));
            if (Equals(null, props)) throw new ArgumentNullException(nameof(props));

            await Init (stateMachineRuntimeParameters.EvaluateSystemTriggers, stateMachineRuntimeParameters.Workflow, false);

            StateMachine<string, string>.TriggerWithParameters<StateManagementCommonTriggerProperties> t;

            if (!_parametrizedTriggers.TryGetValue(trigger, out t))
            {
                throw  new InvalidOperationException($"Required parametrized trigger for trigger : {trigger} was not found");
            }

            //for some states especially for accounting states there are transitions where the guard condition before triger fire doesnt meet, but for GetAllPermittedTransitions met
            var withoutCondition = CheckWithoutCondition(trigger);

            if(withoutCondition)
                using(new StateManagementConditionBypass())
                    await _machine.FireAsync(t, props);
            else
            {
                await _machine.FireAsync(t, props);
            }
        }

        public async Task<TaskRespondStatus> CompleteTaskAsync()
        {
            var sm = await Init(stateMachineRuntimeParameters.EvaluateSystemTriggers, stateMachineRuntimeParameters.Workflow, false);
                        
            var triggerCode = stateMachineRuntimeParameters.UserData.Trigger;

            Guard.StringNotNullOrWhiteSpace(triggerCode, nameof(triggerCode));

            var executingTransition = GetParametrizedTransition(triggerCode);

            _transitionEvaluator.EvaluateUserProperties(executingTransition, stateMachineRuntimeParameters.UserData);

            var currentStateDefinition = GetStateDefinitionByTitle(workflowInstance.WorkflowState);
            var currentStateConfiguration =
                (stateMachineRuntimeParameters.EvaluateSystemTriggers ? sm.SystemTransitions : sm.UserTransitions).FirstOrDefault(c => c.Code.Equals(currentStateDefinition.Code));

            Guard.NotNull(currentStateConfiguration, nameof(currentStateConfiguration), $"Current state configuration was not found, instance state is {workflowInstance.WorkflowState}");
                        
            return await workflowTaskCompletitionEvaluator.CompleteTaskAsync(stateMachineRuntimeParameters, sm, currentStateDefinition, currentStateConfiguration, executingTransition);
        }

        public async Task RollbackTransactionAsync(string triggerCode)
        {
            await Init(stateMachineRuntimeParameters.EvaluateSystemTriggers, stateMachineRuntimeParameters.Workflow, false);

            await Rollback(triggerCode);
        }

        private async Task<StateMachine> Init(bool systemTriggers, WorkflowInfo workflowInfo, bool queryingTransitions)
        {
            _machine = new StateMachine<string, string>(() => workflowInstance.WorkflowState, s => workflowInstance.WorkflowState = s);

            var sm = await _configurationProvider.LoadWorkflowAsync(workflowInstance.Id);

            ConfigureMachine(sm, systemTriggers, queryingTransitions);

            return sm;
        }
                
        private void ConfigureMachine(StateMachine sm, bool systemTriggers, bool queryingTransitions)
        {
            if (sm == null) throw new ArgumentNullException(nameof(sm));

            //Configure definitions first
            ConfigureStates(sm);
                        
            //Configure transitions
            foreach (ConfiguredState configuredState in systemTriggers ? sm.SystemTransitions : sm.UserTransitions)
            {
                //current state matched from state
                var stateDefFrom = GetStateDefinitionByCode(configuredState.Code);

                var matchedAliasCode = stateDefFrom.Aliases?.FirstOrDefault(a => GetStateDefinitionByCode(a).Title.Equals(_machine.State));

                //current state matched one of aliases states
                var matchedAliasFrom = string.IsNullOrEmpty(matchedAliasCode)
                    ? null
                    : GetStateDefinitionByCode(matchedAliasCode);
                
                //Neither state def nor one of the aliases matched current state on metadata
                if (!stateDefFrom.Title.Equals(_machine.State) && Equals(null, matchedAliasFrom)) continue;

                var matchedStateFrom = matchedAliasFrom ?? stateDefFrom;

                if (matchedStateFrom.IsEnd) AddEndState(matchedStateFrom.Title);

                ApplyState(systemTriggers, configuredState.Transitions, matchedStateFrom, queryingTransitions);
            }

            //Configure generic states
            if (!systemTriggers)
            {
                foreach (GenericTransition genericTransition in sm.GenericTransitions)
                {
                    var forStateMatched = genericTransition.ForStates.FirstOrDefault(t => GetStateDefinitionByCode(t)
                        .Title
                        .Equals(_machine.State));

                    if (!Equals(null, forStateMatched))
                    {
                        var stateDefFrom = GetStateDefinitionByCode(forStateMatched);

                        ApplyState(systemTriggers, new List<Configuration.StateMachineConfig.Transitions.Transition> {genericTransition.Transition}, stateDefFrom, queryingTransitions);
                    }
                }
            }
        }

        private void ApplyState(bool isSystemTransitions, List<Configuration.StateMachineConfig.Transitions.Transition> transitions, StateDefinition stateDefFrom, bool queryingTransitions)
        {
            foreach (Configuration.StateMachineConfig.Transitions.Transition stateTransition in transitions)
            {

                bool permissionLessTransition = stateTransition.WithoutPermission;

                string stateTransitionStateCode = stateTransition.State;

                if (stateTransition.State.Equals(STATE_REF_SOURCE_PLACEHOLDER))
                    stateTransitionStateCode = stateDefFrom.Code;

                var stateDefTo = GetStateDefinitionByCode(stateTransitionStateCode);

                string triggerCode = GetTriggerCode(stateDefFrom.Code, stateDefTo.Code,
                    stateTransition.StateManagementTrigger.Order.ToString());

                stateTransition.StateManagementTrigger.Code = triggerCode;
                stateTransition.IsSystem = isSystemTransitions;

                if (!stateDefTo.Code.Equals(stateDefFrom.Code))
                    _machine.Configure(stateDefFrom.Title)
                        .PermitIf(
                            triggerCode,
                            stateDefTo.Title,
                            () =>
                            (Task.Run(() => _transitionEvaluator.EvaluateTransitionConditionAsync(
                                    !isSystemTransitions && queryingTransitions && !permissionLessTransition, BuildEventArgsForTransitionEvaluating(stateTransition))).Result
                            ));

                else
                {
                    stateTransition.IsReentrant = true;

                    _machine.Configure(stateDefFrom.Title)
                        .PermitReentryIf(
                            triggerCode,
                            () => _transitionEvaluator.EvaluateTransitionConditionAsync(
                                !isSystemTransitions && queryingTransitions && !permissionLessTransition, BuildEventArgsForTransitionEvaluating(stateTransition)).GetAwaiter().GetResult());
                }
                

                var paramTrigger = _machine.SetTriggerParameters<StateManagementCommonTriggerProperties>(triggerCode);
                
                AddParametrizedTrigger(triggerCode, paramTrigger);

                AddTriggerTransition(triggerCode, stateTransition);

                _machine.Configure(stateDefTo.Title)
                    .OnEntryFromAsync(paramTrigger, ConfiguredEntryFrom);
            }
        }

        private StateTransitionEventArgs BuildEventArgsForTransitionEvaluating(Configuration.StateMachineConfig.Transitions.Transition transition)
        {
            Guard.NotNull(transition, nameof(transition));

            return new StateTransitionEventArgs(
                stateMachineRuntimeParameters.DocumentMetadataIdentifier.SectionDesignation,
                stateMachineRuntimeParameters.UserData, string.Empty, string.Empty, transition,
                stateMachineRuntimeParameters.DocumentMetadataIdentifier, (int)stateMachineRuntimeParameters.Workflow.DataId, stateMachineRuntimeParameters.UserId, stateMachineRuntimeParameters.UserPermissionGroups, stateMachineRuntimeParameters.EvaluateMembershipLocally);
        }

        private async Task ConfiguredEntryFrom(StateManagementCommonTriggerProperties tranProps, StateMachine<string, string>.Transition tran)
        {
            await DoTransition(tranProps, tran);
        }

        private void ConfigureStates(StateMachine machine)
        {
            var definitions = machine.StateDefinitions;

            if (definitions == null) throw new ArgumentNullException(nameof(definitions));

            foreach (StateDefinition stateDefinition in definitions)
            {
                if (stateDefinitions.ContainsKey(stateDefinition.Code))
                    throw new InvalidOperationException(
                        $"State definition contains multiple definition for state : {stateDefinition.Code}");

                stateDefinitions.Add(stateDefinition.Code, stateDefinition);
            }

            systemStates = machine.SystemTransitions.Select(s => s.Code).ToList();
        }
                
        private bool CheckWithoutCondition(string trigger)
        {
            Configuration.StateMachineConfig.Transitions.Transition tran = GetParametrizedTransition(trigger);

            return tran.IsReentrant;
        }

        private void ValidateRuntimeParameters(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            Guard.NotNull(stateMachineRuntimeParameters, nameof(stateMachineRuntimeParameters));
            Guard.NotNull(stateMachineRuntimeParameters.DocumentMetadataIdentifier, nameof(stateMachineRuntimeParameters.DocumentMetadataIdentifier));
            ValidateDocumentIdentifier(stateMachineRuntimeParameters.DocumentMetadataIdentifier);
            Guard.NotNull(stateMachineRuntimeParameters.Workflow, nameof(stateMachineRuntimeParameters.Workflow));
            ValidateWorkflowInfo(stateMachineRuntimeParameters.Workflow);
            ValidateWorkflowInstance();

            if (!stateMachineRuntimeParameters.EvaluateSystemTriggers && Equals(null, stateMachineRuntimeParameters.UserPermissionGroups)) throw new InvalidOperationException($"User group are null or empty");
            if (!stateMachineRuntimeParameters.EvaluateSystemTriggers && string.IsNullOrWhiteSpace(stateMachineRuntimeParameters.UserId)) throw new InvalidOperationException($"User id is invalid");

            if (Equals(null, stateMachineRuntimeParameters.UserData)) stateMachineRuntimeParameters.UserData = StateManagementCommonTriggerProperties.Empty;
        }

        private void ValidateWorkflowInstance()
        {
            Guard.StringNotNullOrWhiteSpace(workflowInstance.WorkflowState, nameof(workflowInstance.WorkflowState));
        }

        private void ValidateWorkflowInfo(WorkflowInfo workflow)
        {
            Guard.NotNull(workflow, nameof(workflow));
            Guard.NotDefault(workflow.DataId, nameof(workflow.DataId));
        }

        private void ValidateDocument(DocumentWorkflowMetadata workflowDocument)
        {
            Guard.NotNull(workflowDocument, nameof(workflowDocument));
        }

        public static void ValidateDocumentIdentifier(WorkflowDocumentIdentifier workflowDocumentIdentifier)
        {
            Guard.NotNull(workflowDocumentIdentifier, nameof(workflowDocumentIdentifier));
            Guard.NotNull(workflowDocumentIdentifier.SectionDesignation, nameof(workflowDocumentIdentifier.SectionDesignation));
            Guard.NotNull(workflowDocumentIdentifier.Identificator);
            Guard.NotNull(workflowDocumentIdentifier.WorkflowDocumentEntityId);
        }
    }

}
