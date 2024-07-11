using System;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions
{
    [Flags]
    public enum WorkflowPropertyType
    {
        Unknown = 0,
        Metadata = 1,
        Client = 1 << 1,
        Computed = 1 << 2,
        Workflow = 1 << 3
    }
}
