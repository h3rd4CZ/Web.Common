using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    public class TransitionHistory
    {
        public ConditionExpression Condition { get; set; }
        public TransitionHistoryEntry Entry { get; set; } = new TransitionHistoryEntry();
        public TransitionHistoryComment Message { get; set; } = new TransitionHistoryComment();
    }
}
