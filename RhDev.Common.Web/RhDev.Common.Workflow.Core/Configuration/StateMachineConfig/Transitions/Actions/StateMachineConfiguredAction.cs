using RhDev.Common.Workflow.Builder;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System;
using System.Collections.Generic;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions
{
    [Serializable]
    public class StateMachineConfiguredAction : IEquatable<StateMachineConfiguredAction>, IWorkflowPartBuilder
    {
        public string Id { get; set; }

        public ConditionExpression Condition { get; set; }

        public string TypeName { get; set; }

        public bool SuppressException { get; set; }

        public List<StateMachineActionParameter> Parameters { get; set; } = new List<StateMachineActionParameter>();

        public bool Disabled { get; set; }
                
        public bool Equals(StateMachineConfiguredAction other)
        {
            if (ReferenceEquals(other, null)) return false;

            if (ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Id == other.Id;
        }
    }

}
