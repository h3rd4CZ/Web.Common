using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    public class StateManagementBooleanValue : StateManagementValue
    {
        protected override IList<(Regex regex, Func<string, string> evaluator)> Formatters => new List<(Regex regex, Func<string, string> evaluator)>
        {
            (new Regex(@"^[nN]atural;1029$"), s => booleanValue ? "Ano" : "Ne"),
            (new Regex(@"^[nN]atural;1033$"), s => booleanValue ? "Yes" : "No")
        };

        [DataMember]
        bool booleanValue;
        public bool BooleanValue => booleanValue;
        public StateManagementBooleanValue(bool booleanValue) : base(booleanValue)
        {
            this.booleanValue = booleanValue;
        }

        public override StateManagementValue Neg() => new StateManagementBooleanValue(!BooleanValue);

        public override bool ValueEquals(StateManagementValue right)
        {
            if (right.IsNullValue) return false;

            var rightBoolean = Cast<StateManagementBooleanValue>(right);

            return BooleanValue == rightBoolean.BooleanValue;
        }

        public override bool ValueNotEquals(StateManagementValue right) => right.IsNullValue || !ValueEquals(right);

        public override string ToString() => ToStringFormatter(BooleanValue.ToString());
    }
}
