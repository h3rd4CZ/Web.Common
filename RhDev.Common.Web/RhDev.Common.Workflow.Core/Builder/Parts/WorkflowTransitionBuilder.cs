using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using Transition = RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Transition;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowTransitionBuilder : WorkflowPartBuilderBase<WorkflowTransitionBuilder, Transition>
    {
        public const string TransitionIconKey = "TransitionIcon";
        public const string TransitionIconColorKey = "TransitionIconColor";

        public static WorkflowTransitionBuilder New(string state)
        {
            var builder = new WorkflowTransitionBuilder() { };

            builder.Init();
            builder.product.State = state;            

            return builder;
        }
               
        
        public WorkflowTransitionBuilder WithCondition(WorkflowConditionExpressionBuilder builder)
        {
            product.Condition = builder.Build();
            return this;
        }

        public WorkflowTransitionBuilder WithTrigger(WorkflowTriggerBuilder builder)
        {
            product.StateManagementTrigger= builder.Build();
            return this;
        }

        public WorkflowTransitionBuilder WithoutPermission(bool withoutPermissions)
        {
            product.WithoutPermission = withoutPermissions;
            return this;
        }

        public WorkflowTransitionBuilder AddAction(WorkflowCustomActionBuilder builder, bool add = true)
        {
            if (!add) return this;

            product.AdditionalActions.Add(builder.Build());
            return this;
        }
                                
        public WorkflowTransitionBuilder WithTask(WorkflowTaskBuilder builder)
        {
            product.Task= builder?.Build();
            return this;
        }
    }
}
