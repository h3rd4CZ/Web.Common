using Lamar;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Impl;

namespace RhDev.Common.Workflow.Core.Impl
{
    public class CompositionConfiguration : ConventionConfigurationBase
    {
        public CompositionConfiguration(ServiceRegistry configuration, Container container) : base(configuration, container)
        {

        }

        public override void Apply()
        {
            base.Apply();

            ConfigureMachineResolver();

            //For<IWorkflowCompiler<List<WorkflowDefinitionConfiguration>>>().Use<SequenceOfStepsWorkflowCompiler>();

            //(typeof(IWorkflowCompiler<string>)).Use(typeof(SequenceOfStepsWorkflowCompiler));

        }

        private void ConfigureMachineResolver()
        {
            SetInjectbale<StateMachineRuntimeParameters>();
            SetInjectbale<WorkflowInstance>();

            For<Func<StateMachineRuntimeParameters, WorkflowInstance,
            IConfigurableStateMachine<string, StateManagementCommonTriggerProperties>>>().Use(ctx =>
                (rp, wi) =>
                    (IConfigurableStateMachine<string, StateManagementCommonTriggerProperties>)
                        InjectAndGet<StateMachineRuntimeParameters, WorkflowInstance, object, object, object>(
                            typeof(ConfigurableStateMachine),
                            rp,
                            wi,
                            null,
                            null,
                            null));

            For(typeof(IStateManagementProcessor<StateManagementCommonTriggerProperties>))
                .Use(typeof(StateManagementProcessor));
        }
    }
}
