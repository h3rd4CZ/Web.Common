using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration;
using RhDev.Common.Workflow.Core.Management;
using RhDev.Common.Workflow.DataAccess;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Management;

namespace RhDev.Common.Workflow.Impl
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowTransitionRequestEvaluator workflowTransitionRequestEvaluator;
        private readonly ICentralClockProvider clockProvider;
        private readonly IStateManagementProcessor<StateManagementCommonTriggerProperties> processor;

        private readonly IStateManagementDocumentDataRepositoryFacade stateManagementDocumentDataRepositoryFacade;
        private readonly IStateManagementWorkflowConfigurator stateManagementWorkflowConfigurator;
        private readonly ISafeLock safeLock;

        public WorkflowService(
            IWorkflowTransitionRequestEvaluator workflowTransitionRequestEvaluator,
            ICentralClockProvider clockProvider,
            IStateManagementProcessor<StateManagementCommonTriggerProperties> processor,
            IStateManagementDocumentDataRepositoryFacade stateManagementDocumentDataRepositoryFacade,
            IStateManagementWorkflowConfigurator stateManagementWorkflowConfigurator,
            ISafeLock safeLock)
        {
            this.stateManagementWorkflowConfigurator = stateManagementWorkflowConfigurator;
            this.safeLock = safeLock;
            this.workflowTransitionRequestEvaluator = workflowTransitionRequestEvaluator;
            this.clockProvider = clockProvider;
            this.processor = processor;
            this.stateManagementDocumentDataRepositoryFacade = stateManagementDocumentDataRepositoryFacade;
            this.stateManagementWorkflowConfigurator = stateManagementWorkflowConfigurator;
        }

        public async Task DeleteTransitionHistoryAsync(string transitionId)
        {
            await stateManagementDocumentDataRepositoryFacade.DeleteTransitionHistoryAsync(transitionId);
        }

        public async Task<bool> IsDocumentInEndStateAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            return await safeLock.UseDistributedLockForWorkflowDocumentAndReturnAsync(
                stateMachineRuntimeParameters,
                async () => await processor.IsInEndStateAsync(stateMachineRuntimeParameters), nameof(IsDocumentInEndStateAsync));
        }

        public async Task DeleteDocumentAsync(int dataStoreId)
        {
            await stateManagementDocumentDataRepositoryFacade.DeleteDocumentAsync(dataStoreId);
        }

        public async Task<IList<TransitionLogInfo>> GetTransitionLogInfoAsync(string guid)
        {
            return await stateManagementDocumentDataRepositoryFacade.GetTransitionInfoAsync(guid);
        }

        public async Task<List<WorkflowHistoryItem>> GetWorkflowInstanceHistoryAsync(WorkflowInfo workflowInfo)
        {
            return await stateManagementWorkflowConfigurator.GetWorkflowInstanceHistoryAsync(workflowInfo);
        }

        public async Task<TaskCompletitionInfo> GetLastTaskCompletitionInfoAsync(WorkflowInfo workflowInfo, string taskGroupId)
        {
            return await stateManagementWorkflowConfigurator.GetLastTaskCompletitionInfoAsync(workflowInfo, taskGroupId);
        }

        public async Task<WorkflowInfo> StartWorkflowAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, WorkflowDefinitionFile workflowDefinitionFile, string initiatorUserId, List<WorkflowStartProperty> properties)
        {
            return await stateManagementWorkflowConfigurator.StartWorkflowAsync(workflowDocumentIdentifier, workflowDefinitionFile, initiatorUserId, properties);
        }

        public async Task StopWorkflowAsync(WorkflowInfo workflowInfo, string initiatorUserId)
        {
            await stateManagementWorkflowConfigurator.StopWorkflowAsync(workflowInfo, initiatorUserId);
        }

        public async Task<List<WorkflowInfo>> GetAllWorkflowInstancesForDocumentDataIdAsync(int documentDataId, bool includeCompleted)
        {
            return await stateManagementWorkflowConfigurator.GetAllWorkflowInstancesAsync(documentDataId, includeCompleted);
        }

        public async Task WriteHistoryAsync(int workflowInstanceDataId, string user, string eventTitle, string message, DateTime dtm)
        {
            await stateManagementDocumentDataRepositoryFacade.WriteHistoryAsync(workflowInstanceDataId, user, eventTitle, message, dtm);
        }

        public async Task<TaskInfo> DelegateTaskAsync(string delegator, string? delegatedUserHint, StateMachineRuntimeParameters parameters)
        {
            return await safeLock.UseDistributedLockForWorkflowDocumentAndReturnAsync<TaskInfo>(
                parameters,
                async () => await stateManagementDocumentDataRepositoryFacade.DelegateTaskAsync(delegator, delegatedUserHint, parameters),
                nameof(DelegateTaskAsync));
        }

        public async Task CompleteTaskAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {

            await safeLock.UseDistributedLockForWorkflowDocumentAsync(
                stateMachineRuntimeParameters,
                async () =>
                {
                    var result = await processor.CompleteTaskAsync(stateMachineRuntimeParameters);

            if (result == TaskRespondStatus.Fire)
            {
                await EnqueueTransitionRequestAsync(
                    WorkflowTransitionRequestPayload.Create(
                        stateMachineRuntimeParameters,
                        StateTransitionSources.TASK_COMPLETITION,
                        string.Empty), true);
            }
                }, nameof(CompleteTaskAsync));
        }

        public async Task<IList<WorkflowTransitionInfo>> GetAllPermittedTransitionsAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            return await processor.GetAllPermittedTransitionAsync(stateMachineRuntimeParameters);
        }

        public async Task EnqueueTransitionRequestAsync(WorkflowTransitionRequestPayload payload, bool async)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            Guard.NotNull(payload.RuntimeParameters);

            Guard.NotNull(payload, nameof(payload));
            Guard.NotNull(payload.DocumentIdentifier, nameof(payload.DocumentIdentifier));
            Guard.NotNull(payload.DocumentIdentifier.Identificator);
            Guard.NumberMin(payload.DocumentIdentifier.WorkflowDocumentEntityId, 1);
            if (Equals(null, payload.DocumentIdentifier.SectionDesignation)) throw new InvalidOperationException($"Section designation is null");
            
            payload.DocumentIdentifier.Validate();

            Guard.NotNull(payload.TransitionProperties, nameof(payload.TransitionProperties));
            if (!payload.EvaluateSystemTriggers && !payload.TransitionProperties.ValidUserTransitionData)
                throw new InvalidOperationException($"User transition data are not valid. User id must be present as well as execution trigger for given transition");

            if (payload.EvaluateSystemTriggers && !payload.TransitionProperties.ValidSystemTransitionData)
                throw new InvalidOperationException($"System transition data are not valid. Executing trigger must be present");

            Guard.NotNull(payload.Workflow, nameof(payload.Workflow), $"Workflow information missing");

            var requestInitiator = payload.TransitionProperties.UserRespondedId;
            Guard.StringNotNullOrWhiteSpace(requestInitiator);
                        
            var clock = clockProvider.Now();

            var documentReference = payload.DocumentIdentifier.DocumentReference;

            var request = new WorkflowTransitionRequest()
            {
                DocumentReference = documentReference,
                Payload = payload,
                Created = clock.ExportDateTime,
                State = TransitionTaskStatus.Planned,
                Title = string.IsNullOrWhiteSpace(payload.Title) ? $"Workflow transition{(string.IsNullOrWhiteSpace(documentReference) ? string.Empty : $" {documentReference}")}" : payload.Title,
                Source = payload.Source,
                Workflow = payload.Workflow?.Name,
                LastInitiatorId = requestInitiator,
                TransitionType = payload.TransitionType,
            };

            await workflowTransitionRequestEvaluator.EvaluateTransitionAsync(request, async);
        }
    }
}
