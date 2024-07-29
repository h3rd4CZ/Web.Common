using RhDev.Common.Workflow.Configuration.StateMachineConfig;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowConfiguredStateBuilder : WorkflowPartBuilderBase<WorkflowConfiguredStateBuilder, ConfiguredState>
    {
        public static WorkflowConfiguredStateBuilder New(string code)
        {
            var builder = new WorkflowConfiguredStateBuilder() { };

            builder.Init();
            builder.product.Code = code;
            return builder;
        }
              
                
        public WorkflowConfiguredStateBuilder AddTransition(WorkflowTransitionBuilder workflowTransitionBuilder)
        {
            product.Transitions.Add(workflowTransitionBuilder.Build());
            return this;
        }

        public WorkflowConfiguredStateBuilder WithCompletionMail(WorkflowOperandBuilder textBuilder, WorkflowOperandBuilder subjectBuilder)
        {
            product.CompletionMail =
                new Configuration.StateMachineConfig.Transitions.WorkflowTaskMail
                {
                    Subject = subjectBuilder is null ? default : new Configuration.StateMachineConfig.Transitions.WorkflowTaskMailSubject { Operand = subjectBuilder.Build() },
                    Text = textBuilder is null ? default : new Configuration.StateMachineConfig.Transitions.WorkflowTaskMailText { Operand = textBuilder.Build() }
                };

            return this;
        }
    }
}
