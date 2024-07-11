using RhDev.Common.Workflow.Configuration.StateMachineConfig;
using System.Collections.Generic;
using System.Linq;

namespace RhDev.Common.Workflow.Builder.Parts
{
    public class WorkflowStateMachineBuilder : WorkflowPartBuilderBase<WorkflowStateMachineBuilder, StateMachine>
    {
        public static WorkflowStateMachineBuilder New(string version, string comment)
        {
            var builder = new WorkflowStateMachineBuilder() { };

            builder.Init();
            builder.product.Version = version;
            builder.product.VersionComment= comment;
            return builder;
        }
               
        
        public WorkflowStateMachineBuilder AddUserTransition(WorkflowConfiguredStateBuilder builder)
        {
            product.UserTransitions.Add(builder.Build());
            return this;
        }

        public WorkflowStateMachineBuilder AddStateDefinition(WorkflowStateDefinitionBuilder builder)
        {
            product.StateDefinitions.Add(builder.Build());
            return this;
        }

        public WorkflowStateMachineBuilder AddSystemTransition(WorkflowConfiguredStateBuilder builder)
        {
            product.SystemTransitions.Add(builder.Build());
            return this;
        }

        public WorkflowStateMachineBuilder AddGenericTransition(IList<string> forStates, WorkflowTransitionBuilder builder)
        {
            product.GenericTransitions.Add(new GenericTransition { ForStates = forStates.ToList(), Transition = builder.Build() });
            return this;
        }
    }
}
