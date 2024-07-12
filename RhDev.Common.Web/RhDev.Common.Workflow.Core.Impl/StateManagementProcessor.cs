using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Impl.Exceptions;
using RhDev.Common.Workflow.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Workflow;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Workflow.Impl
{
    public class StateManagementProcessor : IStateManagementProcessor<StateManagementCommonTriggerProperties>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;
        private readonly IDynamicDataStoreRepository<DbContext> dynamicDataStore;
        private readonly IWorkflowDocumentRepository workflowDocumentRepository;
        private readonly Func<StateMachineRuntimeParameters, WorkflowInstance, IConfigurableStateMachine<string, StateManagementCommonTriggerProperties>> _stateMachineResolver;
        private readonly IStateTransitionExternalLogging _stateTransitionExternalLogging;

        public StateManagementProcessor(
            IServiceProvider serviceProvider,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IDynamicDataStoreRepository<DbContext> dynamicDataStore,
            IWorkflowDocumentRepository workflowDocumentRepository,
            Func<StateMachineRuntimeParameters, WorkflowInstance,
                IConfigurableStateMachine<string, StateManagementCommonTriggerProperties>> stateMachineResolver,
            IStateTransitionExternalLogging stateTransitionExternalLogging)
        {
            this.serviceProvider = serviceProvider;
            this.workflowInstanceRepository = workflowInstanceRepository;
            this.dynamicDataStore = dynamicDataStore;
            this.workflowDocumentRepository = workflowDocumentRepository;
            _stateMachineResolver = stateMachineResolver;
            _stateTransitionExternalLogging = stateTransitionExternalLogging;
        }

        public async Task<TaskRespondStatus> CompleteTaskAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            Guard.NotNull(stateMachineRuntimeParameters, nameof(stateMachineRuntimeParameters));

            var machine = await GetMachine(stateMachineRuntimeParameters);

            return await machine.CompleteTaskAsync();
        }

        public async Task FireTriggerAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            Guard.NotNull(stateMachineRuntimeParameters, nameof(stateMachineRuntimeParameters));

            var machine = await GetMachine(stateMachineRuntimeParameters);
            await machine.FireParametrizedAsync(stateMachineRuntimeParameters.UserData.Trigger, stateMachineRuntimeParameters.UserData);
            var workflowInstance = machine.WorkflowInstance;

            await SaveWorkflowInstanceState(workflowInstance, machine);
        }

        public async Task<IList<WorkflowTransitionInfo>> GetAllPermittedTransitionAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            Guard.NotNull(stateMachineRuntimeParameters, nameof(stateMachineRuntimeParameters));           
                                    
            var document = await workflowDocumentRepository
                .ReadByIdAsync(stateMachineRuntimeParameters.DocumentMetadataIdentifier.WorkflowDocumentEntityId, new List<Expression<Func<WorkflowDocument, object>>> { d => d.WorkflowInstances });

            var allRunningInstances = document.WorkflowInstances.Where(w => Equals(null, w.Finished));

            if (!Equals(null, stateMachineRuntimeParameters.Workflow) && stateMachineRuntimeParameters.Workflow.DataId > 0)
            {
                allRunningInstances = allRunningInstances.Where(w => w.Id == stateMachineRuntimeParameters.Workflow.DataId);
            }

            var data = new List<WorkflowTransitionInfo> { };

            foreach (var wfInstance in allRunningInstances)
            {
                var wi = new WorkflowInfo
                {
                    DataId = wfInstance.Id,
                    InstanceId = wfInstance.InstanceId,
                    Name = wfInstance.Name,
                    Version = wfInstance.RunVersion,
                    Started = wfInstance.Started,
                    Initiator = wfInstance.InitiatorId
                };

                stateMachineRuntimeParameters.Workflow = wi;

                var machine = await GetMachine(stateMachineRuntimeParameters);

                var allTransitions = await machine.GetAllPermittedTransitionsAsync();

                var wfTranInfo = new WorkflowTransitionInfo
                {
                    WorkflowInfo = wi,
                    TransitionInfos = allTransitions?.Select(BuildTransitionInfo).ToList()
                };

                data.Add(wfTranInfo);
            }

            return data;
        }

        public async Task<bool> IsInEndStateAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            var machine = await GetMachine(stateMachineRuntimeParameters);

            return machine.IsInEndState;
        }

        public async Task RolbackTransactionAsync(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            Guard.NotNull(stateMachineRuntimeParameters, nameof(stateMachineRuntimeParameters));

            ValidateParametersForRollback(stateMachineRuntimeParameters);

            var machine = await GetMachine(stateMachineRuntimeParameters);

            await machine.RollbackTransactionAsync(stateMachineRuntimeParameters.UserData.Trigger); 

        }

        private void ValidateParametersForRollback(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            Guard.NotNull(stateMachineRuntimeParameters, nameof(stateMachineRuntimeParameters));

            if (string.IsNullOrWhiteSpace(stateMachineRuntimeParameters.UserData?.Trigger)) throw new InvalidOperationException("Trigger missing");
        }

        private void ValidateValidDocumentIdentifier(WorkflowDocumentIdentifier documentMetadataIdentifier)
        {
            Guard.NotNull(documentMetadataIdentifier, nameof(documentMetadataIdentifier));

            documentMetadataIdentifier.Validate();
        }
               
        private async Task<WorkflowInstance> GetWorkflowInstance(WorkflowInfo workflow)
        {
            Guard.NotNull(workflow, nameof(workflow));
            Guard.NotDefault(workflow.DataId, nameof(workflow.DataId));
                        
            return await workflowInstanceRepository.ReadByIdAsync((int)workflow.DataId);
        }

        private TransitionInfo BuildTransitionInfo(Configuration.StateMachineConfig.Transitions.Transition transition)
        {
            Guard.NotNull(transition, nameof(transition));
            Guard.NotNull(transition.StateManagementTrigger, nameof(transition.StateManagementTrigger));

            return new TransitionInfo
            {
                Trigger = new TriggerInfo
                {
                    Code = transition.StateManagementTrigger.Code,
                    Name = transition.StateManagementTrigger.Name,
                    Order = transition.StateManagementTrigger.Order,
                    Parameters = transition.StateManagementTrigger.Parameters,
                },
                CustomProperties = transition.CustomProperties
            };
        }

        private async Task SaveWorkflowInstanceState(WorkflowInstance workflowInstance, IConfigurableStateMachine<string, StateManagementCommonTriggerProperties> machine)
        {
            Guard.NotNull(workflowInstance, nameof(workflowInstance));

            string handlerClass = ConfigurableStateMachineBase.SHAREPOINT_UPDATE_HANDLER;

            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            try
            {
                start = DateTime.Now;

                var wi = await workflowInstanceRepository.ReadByIdAsync(workflowInstance.Id);
                wi.WorkflowState = workflowInstance.WorkflowState;
                await workflowInstanceRepository.UpdateAsync(wi);

                end = DateTime.Now;

                await _stateTransitionExternalLogging.LogAsync(handlerClass, default!, machine.StateTransitionEventArgs, start,
                    end, new());
            }
            catch (Exception ex)
            {
                end = DateTime.Now;
                var tExc = new TransitionEvaluationException(ex, handlerClass);
                await _stateTransitionExternalLogging.LogAsync(handlerClass, tExc, machine.StateTransitionEventArgs, start,
                    end, new());

                throw ex;
            }
        }

        async Task<IConfigurableStateMachine<string, StateManagementCommonTriggerProperties>> GetMachine(StateMachineRuntimeParameters stateMachineRuntimeParameters)
        {
            Guard.NotNull(stateMachineRuntimeParameters, nameof(stateMachineRuntimeParameters));

            Guard.NotNull(stateMachineRuntimeParameters.Workflow, nameof(stateMachineRuntimeParameters.Workflow));

            var workflowInstance = await GetWorkflowInstance(stateMachineRuntimeParameters.Workflow);

            var machineType = typeof(ConfigurableStateMachine);

            var ctor = typeof(ConfigurableStateMachine).GetConstructors()
              .FirstOrDefault(c => c.IsPublic && c.GetParameters()?.Length > 0);

            Guard.NotNull(ctor, nameof(ctor), $"Type {machineType} does not contain public constructor with at least 1 argument");

            var ctorParams = ctor.GetParameters();

            var ctorArgObjects = ctorParams.Select(p => p.ParameterType switch
            {
                { } when p.ParameterType == typeof(StateMachineRuntimeParameters) => stateMachineRuntimeParameters,
                { } when p.ParameterType == typeof(WorkflowInstance) => workflowInstance,
                _ => serviceProvider.GetService(p.ParameterType)
            }).ToList();

            var machineObject = Activator.CreateInstance(machineType, ctorArgObjects.ToArray());

            return (IConfigurableStateMachine<string, StateManagementCommonTriggerProperties>)machineObject;
        }
    }

}
