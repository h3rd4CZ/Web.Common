using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Actions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Actions.Definition
{
    public class StateManagementUpdateMetadataAction : StateManagementStateFullActionBase, IStateManagementConfiguredAction
    {
        public StateManagementUpdateMetadataAction(
            IRawEntityDataAccessor rawEntityDataAccessor,
            IPropertyEvaluator propertyEvaluator,
            IWorkflowPropertyManager workflowPropertyManager) : base(propertyEvaluator, workflowPropertyManager)
        {
            RawEntityDataAccessor = rawEntityDataAccessor;
        }

        public static string TypeName => typeof(StateManagementUpdateMetadataAction).FullName;
        public IRawEntityDataAccessor RawEntityDataAccessor { get; }
                
        public async Task RollbackAsync(SectionDesignation designation, string @params) { }
        
        public async Task ExecuteAsync(StateTransitionEventArgs args, List<StateMachineActionParameter> parameters)
        {
            if (Equals(null, parameters)) return;

            IDictionary<string, StateManagementValue> values = new Dictionary<string, StateManagementValue>();

            foreach (var param in parameters)
            {
                var name = param.Name;
                var value = param.Operand;

                Guard.StringNotNullOrWhiteSpace(name);
                Guard.NotNull(value);

                var propertyValue = await PropertyEvaluator.EvaluateAsync(value, args);

                values[name] = propertyValue;
            }

            await RawEntityDataAccessor.SetEntityFieldValuesAndUpdateAsync(args.WorkflowDocumentIdentifier, values);
        }
    }
}
