using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Evaluators
{
    public class PropertyEvaluator : PropertyEvaluatorBase, IPropertyEvaluator
    {
        private readonly IRawEntityDataAccessor rawEntityDataAccessor;
        private readonly IComputeOperandEvaluator computeOperandEvaluator;
        private readonly IWorkflowPropertyManager workflowPropertyManager;

        public PropertyEvaluator(
            IRawEntityDataAccessor rawEntityDataAccessor,
            IUserInfoValueEvaluator userInfoValueEvaluator,
            IComputeOperandEvaluator computeOperandEvaluator,
            IWorkflowPropertyManager workflowPropertyManager, 
            IWorkflowInstanceRepository workflowInstanceRepository) : 
            base(workflowInstanceRepository, userInfoValueEvaluator)
        {
            this.rawEntityDataAccessor = rawEntityDataAccessor;
            this.computeOperandEvaluator = computeOperandEvaluator;
            this.workflowPropertyManager = workflowPropertyManager;
        }

        public async Task<StateManagementValue> EvaluateAsync(Operand property, StateTransitionEventArgs args)
        {
            Guard.NotNull(args, nameof(args));
            Guard.NotNull(property, nameof(property));

            StateManagementValue value = default;

            if (property.Type == WorkflowPropertyType.Metadata)
            {
                value = await rawEntityDataAccessor.GetEntityFieldValueAsync(args.WorkflowDocumentIdentifier, property.Text);

            }
            else if (property.Type == WorkflowPropertyType.Client)
            {
                var source = args.Parameters.TriggerParameters;
                value = await EvaluateKeyValuePairProperty(source, property, args);
            }
            else if (property.Type == WorkflowPropertyType.Computed)
            {
                value = await computeOperandEvaluator.EvaluateAsync(this, property, args);
            }
            else if (property.Type == WorkflowPropertyType.Workflow)
            {
                var propertyName = property.Text;

                Guard.StringNotNullOrWhiteSpace(propertyName, nameof(propertyName), $"Property name is null or empty, property name must be specified using text value of element");

                value = await workflowPropertyManager.LoadPropertyAsync(propertyName, args.WorkflowId);
            }
            else
            {
                value = await EvaluatePlainProperty(this, property, args);
            }

            Guard.NotNull(value, nameof(value));

            if (!string.IsNullOrWhiteSpace(property.Format)) value.SetFormatter(property.Format);

            return value;
        }

        private async Task<StateManagementValue> EvaluateKeyValuePairProperty(
            IList<WorkflowTriggerParameter> source, Operand property,
            StateTransitionEventArgs args)
        {
            Guard.NotNull(property, nameof(property));
            Guard.NotNull(args, nameof(args));
            Guard.StringNotNullOrWhiteSpace(property.Text, nameof(property.Text));

            var propertyText = property.Text;

            var transition = args.Transition;

            var transitionPropertySettings = transition.StateManagementTrigger.Parameters;

            var propertyValue = source?.FirstOrDefault(s => Equals(s.Name, propertyText))?.Value;

            var propertySettings = transitionPropertySettings?.FirstOrDefault(s => s.PropertyName == propertyText);

            return await CastTextPropertyAs(
                this, property, propertyValue,
                Equals(null, propertySettings) ? WorkflowDataType.Unknown : propertySettings.Type, args);
        }
    }
}
