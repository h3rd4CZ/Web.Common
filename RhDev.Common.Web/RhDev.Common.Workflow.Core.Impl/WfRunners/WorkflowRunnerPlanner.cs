using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.Concurrent;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Impl.Utils;
using RhDev.Common.Workflow.Monitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Impl.WfRunners
{
    public class WorkflowRunnerPlanner : IWorkflowRunnerPlanner
    {
        private readonly IWorkflowService workflowService;
        private readonly ICentralClockProvider centralClockProvider;
        private readonly ILogger<WorkflowRunnerPlanner> logger;
        private readonly IDynamicDataStoreRepository<DbContext> dynamicDataStoreRepository;
        private readonly IWorkflowInstanceSystemProcessingInfoRepository workflowInstanceSystemProcessingInfoRepository;
        private readonly IConcurrentDataAccessRepository concurrentDataAccessRepository;
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;

        public WorkflowRunnerPlanner(
           IWorkflowService workflowService,
           ICentralClockProvider centralClockProvider,
           ILogger<WorkflowRunnerPlanner> logger,
           IDynamicDataStoreRepository<DbContext> dynamicDataStoreRepository,
           IWorkflowInstanceSystemProcessingInfoRepository workflowInstanceSystemProcessingInfoRepository,
           IConcurrentDataAccessRepository concurrentDataAccessRepository,
           IWorkflowInstanceRepository workflowInstanceRepository
           )
        {
            this.workflowService = workflowService;
            this.centralClockProvider = centralClockProvider;
            this.logger = logger;
            this.dynamicDataStoreRepository = dynamicDataStoreRepository;
            this.workflowInstanceSystemProcessingInfoRepository = workflowInstanceSystemProcessingInfoRepository;
            this.concurrentDataAccessRepository = concurrentDataAccessRepository;
            this.workflowInstanceRepository = workflowInstanceRepository;
        }
        public async Task PlanAsync(int instanceEntityId)
        {
            Guard.NumberMin(instanceEntityId, 1);

            await concurrentDataAccessRepository.UseIdentifierAsync($"{nameof(WorkflowRunnerPlanner)}_{nameof(PlanAsync)}_{instanceEntityId}", async () =>
            {
                WorkflowInstance instance = default!;
                try
                {
                    logger.LogTrace($"Executing workflow instance runner for ID={instanceEntityId}.");

                    instance = await workflowInstanceRepository.ReadByIdAsync(instanceEntityId, new List<Expression<Func<WorkflowInstance, object>>> { i => i.WorkflowDocument });

                    var clock = centralClockProvider.Now();

                    ValidateInstance(instance);

                    instance.LastSystemProcessing = clock.ExportDateTime;

                    await workflowInstanceRepository.UpdateAsync(instance);

                    var runtimeParameters = await GetRuntimeParameters(instance);

                    var allAvailableTransitions = await workflowService.GetAllPermittedTransitionsAsync(runtimeParameters);

                    var foundTransition = allAvailableTransitions.FirstOrDefault(t => !Equals(null, t.WorkflowInfo) && t.WorkflowInfo.DataId == instanceEntityId);

                    if (!Equals(null, foundTransition))
                    {
                        var allInstanceTransitions = foundTransition.TransitionInfos;

                        if (!Equals(null, allInstanceTransitions))
                        {
                            if (allInstanceTransitions.Count == 1)
                            {
                                var transition = allInstanceTransitions.First();

                                await LogTransitionProcessing(instance, clock, "Valid transition found", transition.ToString(), WorkflowInstanceSystemProcessingInfoItemType.Info);

                                var triggerCode = transition?.Trigger?.Code;
                                Guard.StringNotNullOrWhiteSpace(triggerCode);
                                runtimeParameters.UserData = StateManagementCommonTriggerProperties.System(triggerCode);

                                await workflowService.EnqueueTransitionRequestAsync(WorkflowTransitionRequestPayload.Create(
                                    runtimeParameters,
                                    StateTransitionSources.SYSTEM), false);

                                await LogTransitionProcessing(instance, clock, "Transition executed", transition!.ToString(), WorkflowInstanceSystemProcessingInfoItemType.Info);
                            }

                            if (allInstanceTransitions.Count > 1)
                            {
                                var msg = $"Found multiple transitions for system trigger for instance id : {instance?.Id}, please check workflow configuration file, for system transitions must be only one valid transition";

                                logger.LogWarning(msg);

                                throw new InvalidOperationException(msg);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Workflow instance system processing failed for instance ID: {instance?.Id}, Document reference : {instance?.WorkflowDocument?.DocumentReference}");

                    if (instance is not null) await LogTransitionProcessing(instance!, centralClockProvider.Now(), "Failed", ex.ToString(), WorkflowInstanceSystemProcessingInfoItemType.Fail);
                }
            });
        }

        async Task LogTransitionProcessing(WorkflowInstance wi, CentralClock clock, string header, string message, WorkflowInstanceSystemProcessingInfoItemType infoItemType)
        {
            var info = new WorkflowInstanceSystemProcessingInfo
            {
                WorkflowInstanceId = wi.Id,
                ItemType = infoItemType,
                Stamp = clock.ExportDateTime,
                Header = header,
                Message = message
            };

            await workflowInstanceSystemProcessingInfoRepository.CreateAsync(info);
        }

        async Task<StateMachineRuntimeParameters> GetRuntimeParameters(WorkflowInstance instance)
        {
            Guard.NotNull(instance, nameof(instance));

            var document = instance.WorkflowDocument;
                        
            var sd = SectionDesignation.From(string.Empty);

            var identificator = document.WorkflowDocumentIdentificator;
            var identifier = new WorkflowDocumentIdentifier(sd, identificator, document.DocumentReference, document.Id);

            var @params = new WorkflowRuntimeParametersBuilder()
                .WithDocumentIdentifier(identifier)
                .IsSystem(true)
                .WithUserData(StateManagementCommonTriggerProperties.System(string.Empty))
                .WithUserGroups(new())
                .WithUserId(SystemUserNames.System.ToString())
                .WithWorkflowInfo(new WorkflowInfo
                {
                    DataId = instance.Id,
                    InstanceId = instance.InstanceId,
                    Name = instance.Name,
                    Version = instance.RunVersion
                });

            return @params.Build();
        }

        void ValidateInstance(WorkflowInstance actionDefinition)
        {
            Guard.NotNull(actionDefinition, nameof(actionDefinition));
            Guard.NotNull(actionDefinition.WorkflowDocument);
            Guard.NotNull(actionDefinition.WorkflowDocument.WorkflowDocumentIdentificator);
        }
    }
}
