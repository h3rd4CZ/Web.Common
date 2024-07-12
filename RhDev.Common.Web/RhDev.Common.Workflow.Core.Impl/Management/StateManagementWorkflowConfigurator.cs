using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Utils;
using System.Linq.Expressions;

namespace RhDev.Common.Workflow.Impl.Management
{
    public class StateManagementWorkflowConfigurator : IStateManagementWorkflowConfigurator
    {
        private readonly IDynamicDataStoreRepository<DbContext> dynamicDataStore;
        private readonly IWorkflowTaskRepository workflowTaskRepository;
        private readonly IWorkflowInstanceSystemProcessingInfoRepository workflowInstanceSystemProcessingInfoRepository;
        private readonly IWorkflowDocumentRepository workflowDocumentRepository;
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;
        private readonly IPropertyEvaluator propertyEvaluator;
        private readonly IWorkflowRuntimeParametersBuilder workflowRuntimeParametersBuilder;
        private readonly IWorkflowPropertyManager workflowPropertyManager;

        public StateManagementWorkflowConfigurator(
            IDynamicDataStoreRepository<DbContext> dynamicDataStore,
            IWorkflowTaskRepository workflowTaskRepository,
            IWorkflowInstanceSystemProcessingInfoRepository workflowInstanceSystemProcessingInfoRepository,
            IWorkflowDocumentRepository workflowDocumentRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IPropertyEvaluator propertyEvaluator,
            IWorkflowRuntimeParametersBuilder workflowRuntimeParametersBuilder,
            IWorkflowPropertyManager workflowPropertyManager)
        {
            this.dynamicDataStore = dynamicDataStore;
            this.workflowTaskRepository = workflowTaskRepository;
            this.workflowInstanceSystemProcessingInfoRepository = workflowInstanceSystemProcessingInfoRepository;
            this.workflowDocumentRepository = workflowDocumentRepository;
            this.workflowInstanceRepository = workflowInstanceRepository;
            this.propertyEvaluator = propertyEvaluator;
            this.workflowRuntimeParametersBuilder = workflowRuntimeParametersBuilder;
            this.workflowPropertyManager = workflowPropertyManager;
        }

        public async Task<TaskCompletitionInfo> GetLastTaskCompletitionInfoAsync(WorkflowInfo workflowInfo, string taskGroupId)
        {
            Guard.NotNull(workflowInfo, nameof(workflowInfo));


            var workflowInstance = await workflowInstanceRepository.ReadByIdAsync((int)workflowInfo.DataId, new List<Expression<Func<WorkflowInstance, object>>> { w => w.Tasks });

            var taskGroupData = default(List<WorkflowTask>);

            if (string.IsNullOrWhiteSpace(taskGroupId))
            {
                taskGroupData = workflowInstance.Tasks.GroupBy(t => t.GroupId)
                    .Where(t => !t.Any(task => task.Status == WorkflowDatabaseTaskStatus.NotStarted))
                    .OrderByDescending(g => g.First().AssignedOn)
                    .FirstOrDefault()
                    .ToList();
            }
            else
            {
                taskGroupData =
                    workflowInstance.Tasks.Where(t => t.GroupId == new Guid(taskGroupId)).ToList();
            }

            if (Equals(null, taskGroupData) || taskGroupData.Count == 0) return new TaskCompletitionInfo { NotFound = true };

            var lastUserCompletedTask =
                taskGroupData
                .Where(t => !Equals(null, t.ResolvedOn))
                .OrderByDescending(t => t.ResolvedOn)
                .Where(t => !string.IsNullOrEmpty(t.SelectedTriggerCode))
                .FirstOrDefault();

            if (Equals(null, lastUserCompletedTask)) return new TaskCompletitionInfo { NotFound = true };

            return new TaskCompletitionInfo
            {
                AssignedOn = CentralClock.FillFromDateTime(lastUserCompletedTask.AssignedOn),
                AssignedTo = lastUserCompletedTask.AssignedTo,
                GroupId = lastUserCompletedTask.GroupId.ToString(),
                ResolvedBy = lastUserCompletedTask.ResolvedById,
                ResolvedOn = CentralClock.FillFromDateTime(lastUserCompletedTask.ResolvedOn.Value),
                SelectedTrigger = lastUserCompletedTask.SelectedTriggerCode,
                UserParameters = StateManagementValue.DeserializeCollectionProperties(lastUserCompletedTask.UserData).Select(i => new KeyValuePair<string, string>(i.Name, i.Value.ToString())).ToList()
            };
        }

        public async Task<List<WorkflowHistoryItem>> GetWorkflowInstanceHistoryAsync(WorkflowInfo workflowInfo)
        {
            Guard.NotNull(workflowInfo, nameof(workflowInfo));
            Guard.NotDefault(workflowInfo.DataId, nameof(workflowInfo.DataId));

            var workflowInstance = await workflowInstanceRepository.ReadByIdAsync((int)workflowInfo.DataId, new List<Expression<Func<WorkflowInstance, object>>> { w => w.WorkflowInstanceHistory });

            return workflowInstance.WorkflowInstanceHistory.Select(BuildHistory).ToList();
        }

        public async Task<List<WorkflowInfo>> GetAllWorkflowInstancesAsync(int documentDataId, bool includeCompleted)
        {
            var document =
                await workflowDocumentRepository.ReadByIdAsync(documentDataId,
                    include: new List<Expression<Func<WorkflowDocument, object>>> { d => d.WorkflowInstances });

            var instances = includeCompleted ? document.WorkflowInstances : document.WorkflowInstances.Where(i => Equals(null, i.Finished));
            return instances
                .Select(w => new WorkflowInfo
                {
                    DataId = w.Id,
                    InstanceId = w.InstanceId,
                    Name = w.Name,
                    Version = w.RunVersion,
                    Started = w.Started,
                    Initiator = w.InitiatorId
                }).ToList();
        }

        public async Task StopWorkflowAsync(WorkflowInfo workflowInfo, string initiatorUserId)
        {
            Guard.NotNull(workflowInfo, nameof(workflowInfo));
            Guard.NotDefault(initiatorUserId, nameof(initiatorUserId));
            Guard.NotDefault(workflowInfo.DataId, nameof(workflowInfo.DataId));

            var now = DateTime.Now;

            var wid = workflowInfo.DataId;

            var workflowInstance = await workflowInstanceRepository.ReadByIdAsync((int)wid, new List<Expression<Func<WorkflowInstance, object>>> { w => w.WorkflowDocument });

            if (!Equals(null, workflowInstance.Finished)) throw new InvalidOperationException($"Workflow instance has already been finished at : {workflowInstance.Finished}");

            var parentDocument = workflowInstance.WorkflowDocument;

            workflowInstance.Finished = now;
            await workflowInstanceRepository.UpdateAsync(workflowInstance);

            var allActiveTasks = await workflowTaskRepository.ReadAsync(t => t.WorkflowInstanceId == wid && t.Status == WorkflowDatabaseTaskStatus.NotStarted);
            foreach (var activeTask in allActiveTasks)
            {
                activeTask.Status = WorkflowDatabaseTaskStatus.Completed;
                activeTask.UserData = "WORKFLOW_STOP";
                await workflowTaskRepository.UpdateAsync(activeTask);
            }

            var sysInfo = new WorkflowInstanceSystemProcessingInfo
            {
                WorkflowInstanceId = (int)wid,
                ItemType = WorkflowInstanceSystemProcessingInfoItemType.Info,
                Header = "System",
                Stamp = now,
                Message = $"Workflow has been stopped by user : {initiatorUserId}"
            };

            await workflowInstanceSystemProcessingInfoRepository.CreateAsync(sysInfo);
        }

        public async Task<WorkflowInfo> StartWorkflowAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, WorkflowDefinitionFile workflowDefinitionFile, string initiatorUserId, List<WorkflowStartProperty> properties)
        {
            ConfigurableStateMachine.ValidateDocumentIdentifier(workflowDocumentIdentifier);
            Guard.NotNull(workflowDocumentIdentifier.SectionDesignation);
            Guard.NotNull(workflowDefinitionFile);

            ValidateWorkflowDefinitionFile(workflowDefinitionFile);

            var documentIdentificator = workflowDocumentIdentifier.Identificator;

            var existDocuments =
                await workflowDocumentRepository
                .ReadAsync(d => d.WorkflowDocumentIdentificator.typeName == documentIdentificator.typeName && d.WorkflowDocumentIdentificator.entityId == documentIdentificator.entityId);

            if (existDocuments.Count > 1) throw new InvalidOperationException($"There are to many documents with identifier : {workflowDocumentIdentifier} in database");

            var documentToCreateFor = existDocuments.FirstOrDefault();

            if (Equals(null, documentToCreateFor))
            {
                var newDoc = new WorkflowDocument
                {
                    WorkflowDocumentIdentificator = new WorkflowDocumentIdentificator(documentIdentificator.entityId, documentIdentificator.typeName),
                    DocumentReference = workflowDocumentIdentifier.DocumentReference
                };

                await workflowDocumentRepository.CreateAsync(newDoc);
                documentToCreateFor = newDoc;

                var document = await dynamicDataStore.ReadEntityByIdAsync<IWorkflowDocument>(documentIdentificator.typeName, documentIdentificator.entityId);

                document.WorkflowDocumentId = newDoc.Id;

                await dynamicDataStore.UpdateEntityAsync(document);
            }

            var workflowName = workflowDefinitionFile.Name;

            var existInstance = await workflowInstanceRepository.ReadAsync(w => w.WorkflowDocumentId == documentToCreateFor.Id && w.Name == workflowName && Equals(null, w.Finished));

            if (existInstance.Count > 0) throw new InvalidOperationException($"Workflow with name : {workflowName} has already been running");

            var sm = StateMachine.GetMachine(workflowDefinitionFile.Data);

            var initialState = sm.StateDefinitions?.Where(s => s.IsStart).ToList();

            Guard.CollectionHasExactlyNumberElements(initialState, 1, nameof(initialState), $"No initial state or multiple initial states was found for workflow : {workflowName}");

            var initialStateTitle = initialState.First()?.Title;

            Guard.StringNotNullOrWhiteSpace(initialStateTitle, nameof(initialStateTitle));

            var newInstance = new WorkflowInstance
            {
                WorkflowDocumentId = documentToCreateFor.Id,
                InitiatorId = initiatorUserId,
                InstanceId = Guid.NewGuid().ToString(),
                Name = workflowDefinitionFile.Name,
                RunVersion = sm.Version,
                Started = DateTime.Now,
                WorkflowDefinition = workflowDefinitionFile.Data,
                WorkflowState = initialStateTitle,
                WorkflowStateSystem = true
            };

            await workflowInstanceRepository.CreateAsync(newInstance);

            if (!Equals(null, properties))
                await InitProperties(workflowDocumentIdentifier, initiatorUserId, properties, newInstance);

            return new WorkflowInfo
            {
                DataId = newInstance.Id,
                InstanceId = newInstance.InstanceId,
                Name = newInstance.Name,
                Version = newInstance.RunVersion,
                Started = newInstance.Started,
                Initiator = newInstance.InitiatorId
            };
        }

        private void ValidateWorkflowDefinitionFile(WorkflowDefinitionFile workflowDefinitionFile)
        {
            Guard.NotNull(workflowDefinitionFile, nameof(workflowDefinitionFile));
            Guard.StringNotNullOrWhiteSpace(workflowDefinitionFile.Name, nameof(workflowDefinitionFile.Name));
            Guard.StringNotNullOrWhiteSpace(workflowDefinitionFile.Version, nameof(workflowDefinitionFile.Version));
            Guard.CollectionNotNullAndNotEmpty(workflowDefinitionFile.Data, nameof(workflowDefinitionFile.Data));
        }

        private WorkflowHistoryItem BuildHistory(WorkflowInstanceHistory history)
        {
            Guard.NotNull(history, nameof(history));

            return new WorkflowHistoryItem
            {
                Date = history.Date,
                Event = history.Event,
                Message = history.Message,
                User = history.UserId
            };
        }
        private async Task InitProperties(WorkflowDocumentIdentifier workflowDocumentIdentifier, string initiatorUserId, List<WorkflowStartProperty> properties, WorkflowInstance newInstance)
        {
            var args = new StateTransitionEventArgs
                (workflowDocumentIdentifier.SectionDesignation,
                     StateManagementCommonTriggerProperties.Empty,
                     string.Empty,
                     string.Empty, new Configuration.StateMachineConfig.Transitions.Transition { },
                     workflowDocumentIdentifier,
                     newInstance.Id,
                     initiatorUserId,
                     new List<string> { },
                     false
                );

            var arrayProperties = properties.Where(p => p.Type == WorkflowDataType.Array);
            foreach (var arrayProperty in arrayProperties)
            {
                var arrayItemProperties = properties.Where(p => p.ForArray == arrayProperty.Name);

                var arrayOperand = new Operand
                {
                    DataType = WorkflowDataType.Array,
                    ArrayItemType = arrayProperty.ArrayItemType,
                    Operands = arrayItemProperties
                        .Select(ap => new Operand { DataType = ap.Type, Text = ap.Value, ArrayItemType = ap.ArrayItemType }).ToList()
                };

                var arrayValue = await propertyEvaluator.EvaluateAsync(arrayOperand, args);

                await workflowPropertyManager.SavePropertyAsync(arrayProperty.Name, arrayValue, (int)newInstance.Id);

            }

            var primitiveProperties = properties.Where(p => p.Type != WorkflowDataType.Array && string.IsNullOrWhiteSpace(p.ForArray));
            if (!Equals(null, primitiveProperties) && primitiveProperties.Count() > 0)
            {
                foreach (var property in primitiveProperties)
                {
                    var value = await propertyEvaluator.EvaluateAsync(new Operand
                    {
                        DataType = property.Type,
                        Text = property.Value
                    }, args);

                    await workflowPropertyManager.SavePropertyAsync(property.Name, value, (int)newInstance.Id);
                }

            }
        }
    }
}
