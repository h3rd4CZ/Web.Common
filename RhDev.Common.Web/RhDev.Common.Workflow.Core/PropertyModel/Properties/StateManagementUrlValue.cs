using RhDev.Common.Web.Core.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    public class StateManagementUrlValue : StateManagementValue
    {
        protected override IList<(Regex regex, Func<string, string> evaluator)> Formatters => new List<(Regex regex, Func<string, string> evaluator)>
        {
            (new Regex(@"^[lL]ink"), s => link),
            (new Regex(@"^[dD]escription"), s => description)
        };

        [DataMember]
        private string link;
        [DataMember]
        private string description;

        public string Link => link;
        public string Description => description;
        public StateManagementUrlValue(string link, string description) : base(link)
        {
            Guard.NotNull(link, nameof(link));
            Guard.NotNull(description, nameof(description));

            this.link= link;
            this.description = description;
        }

        public override bool NotEmpty() => !string.IsNullOrWhiteSpace(link);

        public override bool ValueEquals(StateManagementValue right)
        {
            if (right.IsNullValue) return false;

            var rightUrl = Cast<StateManagementUrlValue>(right);

            return link == rightUrl.Link;
        }
        public override bool ValueNotEquals(StateManagementValue right) => right.IsNullValue || !ValueEquals(right);

        public override bool Empty() => !NotEmpty();

        
        public override StateManagementValue Format(List<StateManagementValue> parameters)
        {
            var labelText = new StateManagementTextValue(link);

            return labelText.Format(parameters);
        }

        public override StateManagementValue Length() => new StateManagementNumberValue(link.Length);      
        
        public override string ToString() => ToStringFormatter(string.IsNullOrWhiteSpace(description) ? link : description);
    }
}
