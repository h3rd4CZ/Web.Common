using RhDev.Common.Workflow.Configuration.StateMachineConfig;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Workflow.Impl
{
    public class StateMachineConfigurationProvider : IStateMachineConfigurationProvider
    {
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;

        public StateMachineConfigurationProvider(IWorkflowInstanceRepository workflowInstanceRepository)
        {
            this.workflowInstanceRepository = workflowInstanceRepository;
        }
        public async Task<StateMachine> LoadWorkflowAsync(int workflowInstanceId)
        {
            Guard.NumberMin(workflowInstanceId, 1, nameof(workflowInstanceId));          
                        
            var workflowInstance = await workflowInstanceRepository.ReadByIdAsync(workflowInstanceId);

            var wfData = workflowInstance.WorkflowDefinition;

            return StateMachine.GetMachine(wfData);
        }
    }
}
