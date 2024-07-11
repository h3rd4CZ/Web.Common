using RhDev.Common.Workflow.Builder;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    [Serializable]
    public class StateManagementTrigger : IWorkflowPartBuilder
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public int Order { get; set; }

        public List<TransitionHistory> History { get; set; } = new List<TransitionHistory>();

        public List<TransitionParameter> Parameters { get; set; } = new List<TransitionParameter>();
    }
}
