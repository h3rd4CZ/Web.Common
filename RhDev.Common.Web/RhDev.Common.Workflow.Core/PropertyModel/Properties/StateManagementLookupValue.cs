using RhDev.Common.Web.Core.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    public class StateManagementLookupValue : StateManagementValue
    {
        private bool _wasBuild = false;
        protected override IList<(Regex regex, Func<string, string> evaluator)> Formatters => new List<(Regex regex, Func<string, string> evaluator)>
        {
            (new Regex(@"^[lL]abel$"), s => label),
            (new Regex(@"^[iI]d"), s => id.ToString())
        };

        [DataMember]
        private Guid lookupListId;
        [DataMember]
        private double id;
        [DataMember]
        private string label;

        public Guid LookupListId => lookupListId;
        public double Id => id;
        public string Label => label;
                
        public StateManagementLookupValue(Guid lookupListId, double id, string label) : base(id)
        {
            Guard.NotDefault(lookupListId, nameof(lookupListId));
            Guard.NumberMin(id, 1, nameof(id));

            this.lookupListId = lookupListId;
            this.id = id;
            this.label = label;
        }

        public override bool Equals(object obj)
        {
            return obj is StateManagementLookupValue value && Equals(lookupListId, value.lookupListId) && id == value.Id;
        }

        public override int GetHashCode()
        {
            int hashCode = -1249983435;
            hashCode = hashCode * -1521134295 + lookupListId.GetHashCode();
            hashCode = hashCode * -1521134295 + id.GetHashCode();
            return hashCode;
        }

        public override bool ValueEquals(StateManagementValue right)
        {
            if (right.IsNullValue) return false;

            var rightLookup = Cast<StateManagementLookupValue>(right);

            return Equals(rightLookup);
        }

        public override bool ValueNotEquals(StateManagementValue right) => right.IsNullValue || !ValueEquals(right);

        public override string ToString() => ToStringFormatter(label);
               
    }
}
