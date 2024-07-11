using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow
{
    public interface IRawEntityDataAccessor : IAutoregisteredService
    {
        Task<StateManagementValue> GetEntityFieldValueAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, string fieldInternal);
        Task SetEntityFieldValuesAndUpdateAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, IDictionary<string, StateManagementValue> values);
    }
}
