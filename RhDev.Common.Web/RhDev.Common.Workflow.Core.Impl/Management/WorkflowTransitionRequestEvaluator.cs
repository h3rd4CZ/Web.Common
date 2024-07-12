using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Impl.Timer.Queue;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Core.Management;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.Utils;

namespace RhDev.Common.Workflow.Core.Impl.Management
{
    public class WorkflowTransitionRequestEvaluator : IWorkflowTransitionRequestEvaluator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ISafeLock safeLock;
        private readonly IBackgroundTaskQueue backgroundTaskQueue;
        private readonly IWorkflowRuntimeParametersBuilder workflowRuntimeParametersBuilder;
        private readonly ICentralClockProvider centralClockProvider;
        private readonly IWorkflowTransitionRequestRepository workflowTransitionRequestRepository;
        private readonly IStateManagementProcessor<StateManagementCommonTriggerProperties> outerProcessor;

        public WorkflowTransitionRequestEvaluator(
            IServiceProvider serviceProvider,
            ISafeLock safeLock,
            IBackgroundTaskQueue backgroundTaskQueue,
            IWorkflowRuntimeParametersBuilder workflowRuntimeParametersBuilder,
            ICentralClockProvider centralClockProvider,
            IWorkflowTransitionRequestRepository workflowTransitionRequestRepository,
            IStateManagementProcessor<StateManagementCommonTriggerProperties> outerProcessor)
        {
            this.serviceProvider = serviceProvider;
            this.safeLock = safeLock;
            this.backgroundTaskQueue = backgroundTaskQueue;
            this.workflowRuntimeParametersBuilder = workflowRuntimeParametersBuilder;
            this.centralClockProvider = centralClockProvider;
            this.workflowTransitionRequestRepository = workflowTransitionRequestRepository;
            this.outerProcessor = outerProcessor;
        }

        public async Task EvaluateTransitionAsync(WorkflowTransitionRequest request, bool async)
        {
            if (async)
            {
                await backgroundTaskQueue.QueueBackgroundWorkItemAsync(new(
                    $"{nameof(WorkflowTransitionRequestEvaluator)}_{request.DocumentReference}",
                    async t => await EvaluateTransition(request, serviceProvider)));
            }
            else await EvaluateTransition(request, serviceProvider);
        }

        async Task EvaluateTransition(WorkflowTransitionRequest request, IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var workflowInstanceRepository = serviceScope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
                var processor = serviceScope.ServiceProvider.GetRequiredService<IStateManagementProcessor<StateManagementCommonTriggerProperties>>();

                var requestEntity = request;
                var payload = request.Payload;
                var runtimeParmeters = payload.RuntimeParameters;
                var metadataIdentifier = payload.DocumentIdentifier;
                var instanceId = payload.Workflow.DataId;

                if (request.HasBeenCreated && request.State == TransitionTaskStatus.Failed)
                {
                    var rollbackInfo = request.Payload.RollbackInfo;

                    if (rollbackInfo is null || !rollbackInfo.successfully) throw new InvalidOperationException("Repetation evaluation of request is only available for requests with completed rollback");
                }

                var tranId = Guid.NewGuid();
                requestEntity.TransitionId = tranId;
                payload.TransitionProperties.TransitionId = tranId;

                Guard.NotNull(requestEntity, nameof(requestEntity));

                var ellapsed = default(long);

                WorkflowInstance wfInstance = default!;

                try
                {
                    wfInstance = await workflowInstanceRepository.ReadByIdAsync((int)instanceId);

                    using (MeasureTime.Start(sw => ellapsed = sw.ElapsedMilliseconds))
                    {
                        await ExecuteTransition(runtimeParmeters, processor);
                    }

                    requestEntity.State = TransitionTaskStatus.Completed;
                    requestEntity.ResultData = string.Empty;

                    if (wfInstance.IsFailed)
                    {
                        wfInstance = await workflowInstanceRepository.ReadByIdAsync((int)instanceId);
                        wfInstance.IsFailed = false;
                        await workflowInstanceRepository.UpdateAsync(wfInstance);
                    }
                }
                catch (Exception ex)
                {
                    requestEntity.State = TransitionTaskStatus.Failed;
                    requestEntity.ResultData = ex.ToString();
                    requestEntity.Payload.RollbackInfo = default!;
                    requestEntity.RepeatCount = requestEntity.RepeatCount + 1;

                    if (!wfInstance!.IsFailed)
                    {
                        wfInstance = await workflowInstanceRepository.ReadByIdAsync((int)instanceId);
                        wfInstance.IsFailed = true;
                        await workflowInstanceRepository.UpdateAsync(wfInstance);
                    }
                }
                finally
                {
                    requestEntity.TranDuration = ellapsed;
                    requestEntity.Finished = centralClockProvider.Now().ExportDateTime;
                }

                if (requestEntity.HasBeenCreated) await workflowTransitionRequestRepository.UpdateAsync(requestEntity);
                else
                {
                    requestEntity.Created = centralClockProvider.Now().ExportDateTime;
                    await workflowTransitionRequestRepository.PushRequestAsync(requestEntity);
                }
            }
        }

        public async Task RollbackTransitionAsync(int transitionRequestEntityId)
        {
            Guard.NumberMin(transitionRequestEntityId, 1);

            var transitionRequest = await workflowTransitionRequestRepository.ReadByIdAsync(transitionRequestEntityId);
            Guard.NotNull(transitionRequest.Payload!);
            Guard.NotNull(transitionRequest.Payload.RuntimeParameters!);
            var runtimeParams = transitionRequest.Payload?.RuntimeParameters;
            var workflowInstanceId = transitionRequest.Payload!.Workflow.DataId;
            Guard.NumberMin(workflowInstanceId, 1);

            var rollbackInfo = transitionRequest.Payload.RollbackInfo;

            if (rollbackInfo is not null && rollbackInfo.successfully) throw new InvalidOperationException($"Rollback has already been done at {rollbackInfo.rollbackedAt}");

            try
            {
                await RollbackTransaction(runtimeParams!);

                transitionRequest.Payload.RollbackInfo = new(centralClockProvider.Now().ExportDateTime, true, string.Empty);
                await workflowTransitionRequestRepository.UpdateAsync(transitionRequest);
            }
            catch (Exception ex)
            {
                transitionRequest.Payload.RollbackInfo = new(centralClockProvider.Now().ExportDateTime, false, ex.ToString());
                await workflowTransitionRequestRepository.UpdateAsync(transitionRequest);
            }
        }

        async Task ExecuteTransition(StateMachineRuntimeParameters runtimeParameters, IStateManagementProcessor<StateManagementCommonTriggerProperties> processor)
        {
            //await processor.FireTriggerAsync(runtimeParameters);
            await safeLock.UseDistributedLockForWorkflowDocumentAsync(
                runtimeParameters,
                async () => await processor.FireTriggerAsync(runtimeParameters), nameof(ExecuteTransition));
        }

        async Task RollbackTransaction(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            //await processor.RolbackTransactionAsync(stateMachineRuntimeParameters);
            await safeLock.UseDistributedLockForWorkflowDocumentAsync(
                stateMachineRuntimeParameters,
                () => outerProcessor.RolbackTransactionAsync(stateMachineRuntimeParameters), nameof(RollbackTransaction));
        }
    }
}
