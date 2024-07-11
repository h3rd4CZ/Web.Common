using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    public class StateManagementNullValue : StateManagementValue
    {
        public StateManagementNullValue() : base(null!)
        {
        }

        public override bool IsNull() => true;
        public override bool IsNotNull() => false;
        public override bool GreaterThenOrEqual(StateManagementValue right) => false;
        public override bool CurrentUserInGroup(IList<string> currentUsersGroup) => false;
        public override bool Empty() => true;
        public override bool GreaterThen(StateManagementValue stateManagementValue) => false;
        public override bool LessThen(StateManagementValue stateManagementValue) => false;
        public override bool LessThenOrEqual(StateManagementValue right) => false;
        public override bool ValueEquals(StateManagementValue right) => false;
        public override bool ValueNotEquals(StateManagementValue right) => true;

        public override string ToString() => string.Empty;
        public static StateManagementValue Deserialize(string value) => new StateManagementNullValue();
    }
}
