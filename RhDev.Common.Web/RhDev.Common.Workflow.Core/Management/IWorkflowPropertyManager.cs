using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Management
{
    public interface IWorkflowPropertyManager : IAutoregisteredService
    {
        Task SavePropertyAsync(string name, StateManagementValue value, int worklowId);
        Task<StateManagementValue> LoadPropertyAsync(string name, int worklowId);
    }
}
