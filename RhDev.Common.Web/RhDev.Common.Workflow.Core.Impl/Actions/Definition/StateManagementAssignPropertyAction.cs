using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Actions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Impl.Management;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Actions.Definition
{
    public class StateManagementAssignPropertyAction : StateManagementStateFullActionBase, IStateManagementConfiguredAction
    {
        public StateManagementAssignPropertyAction(
            IComputeOperandEvaluator computeOperandEvaluator,
            IPropertyEvaluator propertyEvaluator,
            IWorkflowPropertyManager workflowPropertyManager) : base(propertyEvaluator, workflowPropertyManager)
        {
            ComputeOperandEvaluator = computeOperandEvaluator;
        }

        public IComputeOperandEvaluator ComputeOperandEvaluator { get; }

        
        public async Task RollbackAsync(SectionDesignation designation, string @params) { }
        
        public async Task ExecuteAsync(StateTransitionEventArgs args, List<StateMachineActionParameter> parameters)
        {
            if (parameters is not null)
            {
                foreach (var parameter in parameters)
                {
                    Guard.StringNotNullOrWhiteSpace(parameter.Name);

                    if(parameter.Operand is not null)
                    {
                        var value = await PropertyEvaluator.EvaluateAsync(parameter.Operand, args);

                        await WorkflowPropertyManager.SavePropertyAsync(parameter.Name, value, args.WorkflowId);
                    }
                }
            }
        }
    }
}
