using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowPermissionSetBuilder : WorkflowPartBuilderBase<WorkflowPermissionSetBuilder, PermissionSet>
    {                
        public WorkflowPermissionSetBuilder WithPermission(string level, WorkflowOperandBuilder operandBuilder)
        {
            product.Level = level;
            product.Operand = operandBuilder.Build();
            return this;
        }
    }
}
