using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    public class StateManagementDateTimeValue : StateManagementValue
    {
        protected override IList<(Regex regex, Func<string, string> evaluator)> Formatters => new List<(Regex regex, Func<string, string> evaluator)>
        {
            (new Regex(@"^[sS]hort[tT]ime$"), s => dateTime.ToShortTimeString()),
            (new Regex(@"^[sS]hort[dD]ate$"), s => dateTime.ToShortDateString()),
            (new Regex(@"^[lL]ong[tT]ime$"), s => dateTime.ToLongTimeString()),
            (new Regex(@"^[lL]ong[dD]ate$"), s => dateTime.ToLongDateString()),
            (new Regex(@"^[sS]hort[tT]ime;\d{4}$"), s => 
            {
                var ci = new CultureInfo(Convert.ToInt32(s.Split(';')[1]));
                return dateTime.ToString(ci.DateTimeFormat.ShortTimePattern);
            }),
            (new Regex(@"^[sS]hort[dD]ate;\d{4}$"), s =>
            {
                var ci = new CultureInfo(Convert.ToInt32(s.Split(';')[1]));
                return dateTime.ToString(ci.DateTimeFormat.ShortDatePattern);
            }),
            (new Regex(@"^[lL]ong[tT]ime;\d{4}$"), s =>
            {
                var ci = new CultureInfo(Convert.ToInt32(s.Split(';')[1]));
                return dateTime.ToString(ci.DateTimeFormat.LongTimePattern);
            }),
            (new Regex(@"^[lL]ong[dD]ate;\d{4}$"), s =>
            {
                var ci = new CultureInfo(Convert.ToInt32(s.Split(';')[1]));
                return dateTime.ToString(ci.DateTimeFormat.LongDatePattern);
            }),
            (new Regex(@"^\d{4}$"), s => dateTime.ToString(new CultureInfo(Convert.ToInt32(s)))),
            (new Regex(@"^.[^;]+;\d{4}$"), s => dateTime.ToString(s.Split(';')[0], new CultureInfo(Convert.ToInt32( s.Split(';')[1])))),
            (new Regex(@"^.+$"), s => dateTime.ToString(s)),
        };

        [DataMember]
        private DateTime dateTime;

        public DateTime DateTime => dateTime; 
        public StateManagementDateTimeValue(DateTime dateTime) : base(dateTime)
        {
            this.dateTime = dateTime;
        }

        public override bool LessThenOrEqual(StateManagementValue right) => !right.IsNullValue && !GreaterThen(right);

        public override bool GreaterThen(StateManagementValue stateManagementValue)
        {
            if (stateManagementValue.IsNullValue) return false;

            var rightDateTime = Cast<StateManagementDateTimeValue>(stateManagementValue);

            return DateTime > rightDateTime.DateTime;
        }


        public override bool GreaterThenOrEqual(StateManagementValue right) => !right.IsNullValue && !LessThen(right);

        public override bool LessThen(StateManagementValue stateManagementValue)
        {
            if (stateManagementValue.IsNullValue) return false;

            var rightDateTime = Cast<StateManagementDateTimeValue>(stateManagementValue);

            return DateTime < rightDateTime.DateTime;
        }

        public override bool ValueEquals(StateManagementValue right)
        {
            if (right.IsNullValue) return false;

            var rightDateTime = Cast<StateManagementDateTimeValue>(right);

            return DateTime == rightDateTime.DateTime;
        }

        public override StateManagementValue AddYears(StateManagementValue days) => UpdateDate(days, d => DateTime.AddYears((int)d.NumberValue));
        public override StateManagementValue AddMonths(StateManagementValue days) => UpdateDate(days, d => DateTime.AddMonths((int)d.NumberValue));
        public override StateManagementValue AddDays(StateManagementValue days) => UpdateDate(days, d => DateTime.AddDays((int)d.NumberValue));
        public override StateManagementValue AddHours(StateManagementValue days) => UpdateDate(days, d => DateTime.AddHours((int)d.NumberValue));
        public override StateManagementValue AddMinutes(StateManagementValue days) => UpdateDate(days, d => DateTime.AddMinutes((int)d.NumberValue));

        public StateManagementValue UpdateDate(StateManagementValue days, Func<StateManagementNumberValue, DateTime> dtmFactory)
        {
            var number = Cast<StateManagementNumberValue>(days);

            return new StateManagementDateTimeValue(dtmFactory(number));
        }

        public override bool ValueNotEquals(StateManagementValue right) => right.IsNullValue || !ValueEquals(right);

        public override string ToString() => ToStringFormatter(DateTime.ToString());
    }
}
