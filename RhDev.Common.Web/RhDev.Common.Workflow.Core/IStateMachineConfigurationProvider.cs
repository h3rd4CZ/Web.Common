using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Workflow.Configuration.StateMachineConfig;
using RhDev.Common.Workflow.Management;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow
{
    public interface IStateMachineConfigurationProvider : IAutoregisteredService
    {
        Task<StateMachine> LoadWorkflowAsync(int workflowInstanceId);
    }
}
