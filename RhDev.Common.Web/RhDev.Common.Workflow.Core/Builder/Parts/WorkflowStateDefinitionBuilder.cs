using RhDev.Common.Workflow.Configuration.StateMachineConfig.StateDefinition;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowStateDefinitionBuilder : WorkflowPartBuilderBase<WorkflowStateDefinitionBuilder, StateDefinition>
    {
        public static WorkflowStateDefinitionBuilder New(string title, string code, bool isEnd, bool isStart)
        {
            var builder = new WorkflowStateDefinitionBuilder() { };

            builder.Init();
            builder.product.Code = code;
            builder.product.Title= title;
            builder.product.IsEnd= isEnd;
            builder.product.IsStart= isStart;

            return builder;
        }
    }
}
