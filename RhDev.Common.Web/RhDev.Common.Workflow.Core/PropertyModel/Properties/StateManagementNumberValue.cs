using System;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    public class StateManagementNumberValue : StateManagementValue
    {
        protected override IList<(Regex regex, Func<string, string> evaluator)> Formatters => new List<(Regex regex, Func<string, string> evaluator)>
        {
            (new Regex(@"^\d{4}$"), s => numberValue.ToString(new CultureInfo(Convert.ToInt32(s)))),
            (new Regex(@"^.[^;]+;\d{4}$"), s => numberValue.ToString(s.Split(';')[0], new CultureInfo(Convert.ToInt32( s.Split(';')[1])))),
            (new Regex(@"^.+$"), s => numberValue.ToString(s)),
        };

        [DataMember]
        double numberValue;

        public double NumberValue => numberValue;
        public StateManagementNumberValue(double doubleValue) : base(doubleValue)
        {
            this.numberValue = doubleValue;
        }

        public override bool LessThenOrEqual(StateManagementValue right) => !right.IsNullValue && !GreaterThen(right);

        public override bool GreaterThen(StateManagementValue stateManagementValue)
        {
            if (stateManagementValue.IsNullValue) return false;

            var rightDouble = Cast<StateManagementNumberValue>(stateManagementValue);

            return NumberValue > rightDouble.NumberValue;
        }


        public override bool GreaterThenOrEqual(StateManagementValue right) => !right.IsNullValue && !LessThen(right);

        public override bool LessThen(StateManagementValue stateManagementValue)
        {
            if (stateManagementValue.IsNullValue) return false;

            var rightDouble = Cast<StateManagementNumberValue>(stateManagementValue);

            return NumberValue < rightDouble.NumberValue;
        }

        public override bool ValueEquals(StateManagementValue right)
        {
            if (right.IsNullValue) return false;

            var rightDouble = Cast<StateManagementNumberValue>(right);

            return NumberValue == rightDouble.NumberValue;
        }
                
        public override StateManagementValue Add(StateManagementValue right) => EvaluateArithmetic(right, (l, r) => l + r);
        public override StateManagementValue Sub(StateManagementValue right) => EvaluateArithmetic(right, (l, r) => l - r);
        public override StateManagementValue Mul(StateManagementValue right) => EvaluateArithmetic(right, (l, r) => l * r);
        public override StateManagementValue Div(StateManagementValue right) => EvaluateArithmetic(right, (l, r) =>
        { if (r == 0) throw new DivideByZeroException(); return l / r; });
        public override StateManagementValue Abs()
        {
            var abs = Math.Abs(NumberValue);

            return new StateManagementNumberValue(abs);
        }
        
        public override bool ValueNotEquals(StateManagementValue right) => right.IsNullValue || !ValueEquals(right);

        public static StateManagementValue Deserialize(string value) => new StateManagementNumberValue(double.Parse(value));

        public override string ToString() => ToStringFormatter(NumberValue.ToString());

        StateManagementNumberValue EvaluateArithmetic(StateManagementValue right, Func<double, double, double> evaluator)
        {
            if (right.IsNullValue) return this;

            var rightDouble = Cast<StateManagementNumberValue>(right);

            return new StateManagementNumberValue(evaluator(NumberValue, rightDouble.NumberValue));
        }
    }
}
