using RhDev.Common.Web.Core.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    public class StateManagementTextValue : StateManagementValue
    {
        [DataMember]
        private string textValue;

        public string TextValue => textValue;
        public StateManagementTextValue(string textValue) : base(textValue)
        {
            Guard.NotNull(textValue, nameof(textValue));

            this.textValue = textValue;
        }

        public override bool NotEmpty() => !string.IsNullOrWhiteSpace(TextValue);

        public override bool ValueEquals(StateManagementValue right)
        {
            if (right.IsNullValue) return false;

            var rightText = Cast<StateManagementTextValue>(right);

            return TextValue == rightText.TextValue;
        }
        public override bool ValueNotEquals(StateManagementValue right) => right.IsNullValue || !ValueEquals(right);

        public override bool Empty() => string.IsNullOrEmpty(TextValue);

        public override StateManagementValue Substring(StateManagementValue start, StateManagementValue length)
        {
            var startVal = Cast<StateManagementNumberValue>(start);

            var startValDouble = startVal.NumberValue;

            if (startValDouble < 0 || startValDouble > TextValue.Length - 1) throw new InvalidOperationException("Start operand must be greater then 0 and less then sizae of the text");

            var lengthDouble = !Equals(null, length) && length is StateManagementNumberValue num ? num.NumberValue : 0;

            return lengthDouble > 0 ?
                new StateManagementTextValue(TextValue.Substring((int)startValDouble, (int)lengthDouble)) :
                new StateManagementTextValue(TextValue.Substring((int)startValDouble));
        }

        public override StateManagementValue Format(List<StateManagementValue> parameters)
        {

            if (Equals(null, parameters) || parameters.Count == 0) return this;

            var regex = new Regex(@"{\d+}");

            var paramsMatchees = regex.Matches(textValue);

            if (paramsMatchees.Count == 0 || Equals(null, parameters)) return this;

            var textToReplace = textValue;

            foreach (Match match in paramsMatchees)
            {
                if (!match.Success) continue;
                var matchString = match.Value;
                var indexString = matchString.Substring(1, matchString.Length - 2);
                if (!int.TryParse(indexString, out int index)) throw new InvalidOperationException($"Parsing indexString {matchString} as number failed");

                if (index < 0 || index >= parameters.Count) continue;

                var propertyAtIndex = parameters[index];

                var replacement = Equals(null, propertyAtIndex) ? string.Empty : propertyAtIndex.ToString();

                textToReplace = textToReplace.Replace(match.Value, replacement);
            }

            return new StateManagementTextValue(textToReplace);
        }

        public override StateManagementValue Length() => new StateManagementNumberValue(TextValue.Length);

        public override string ToString() => TextValue;

        public static StateManagementValue Deserialize(string value) => new StateManagementTextValue(value);
    }
}
