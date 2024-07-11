using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowTaskBuilder : WorkflowPartBuilderBase<WorkflowTaskBuilder, WorkflowAssigneeTask>
    {
        public static WorkflowTaskBuilder New(WorkflowTaskRespondType taskRespondeType, bool groupExtract)
        {
            var builder = new WorkflowTaskBuilder() { };

            builder.Init();
            builder.product.GroupExtract= groupExtract;
            builder.product.TaskRespondeType = taskRespondeType;

            return builder;
        }
               
        
        public WorkflowTaskBuilder AddAssignee(WorkflowOperandBuilder builder)
        {
            product.Assignee.Add(builder.Build());
            return this;
        }

        public WorkflowTaskBuilder AddAssignees(List<WorkflowOperandBuilder> builder)
        {
            product.Assignee.AddRange(builder.Select(b => b.Build()));
            return this;
        }

        public WorkflowTaskBuilder AddPermission(string level, WorkflowOperandBuilder builder)
        {
            product.Permission.Add(new PermissionSet { Level = level, Operand = builder.Build() });
            return this;
        }

        public WorkflowTaskBuilder AddPermissions(List<WorkflowPermissionSetBuilder> workflowPermissionSetBuilder)
        {
            product.Permission.AddRange(workflowPermissionSetBuilder.Select(b => b.Build()).ToList());
            return this;
        }

        public WorkflowTaskBuilder AddMail(WorkflowOperandBuilder textBuilder, WorkflowOperandBuilder subjectBuilder)
        {
            product.Mail.Subject.Operand = subjectBuilder.Build();
            product.Mail.Text.Operand = textBuilder.Build();
            return this;
        }
    }
}
