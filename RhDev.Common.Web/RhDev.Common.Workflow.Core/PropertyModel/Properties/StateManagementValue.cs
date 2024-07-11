using RhDev.Common.Web.Core.Utils;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    //[Serializable]
    [DataContract]
    [KnownType(typeof(StateManagementArrayValue))]
    [KnownType(typeof(StateManagementBooleanValue))]
    [KnownType(typeof(StateManagementDateTimeValue))]
    [KnownType(typeof(StateManagementNullValue))]
    [KnownType(typeof(StateManagementNumberValue))]
    [KnownType(typeof(StateManagementTextValue))]
    [KnownType(typeof(StateManagementUserValue))]
    public abstract class StateManagementValue
    {
        
        protected CultureInfo CZ = new CultureInfo(1029);
        
        protected CultureInfo EN = new CultureInfo(1033);
                
        protected virtual IList<(Regex regex, Func<string, string> evaluator)> Formatters { get; }
            = new List<(Regex regex, Func<string, string> evaluator)> { };
        public static StateManagementValue Null => new StateManagementNullValue();

        protected readonly object _underlayingObject;
        
        protected string _formatter;

        public void SetFormatter(string formatter) => _formatter = formatter;

        public virtual bool IsNullValue => GetType() == typeof(StateManagementNullValue);

        protected StateManagementValue(object underlaying)
        {
            _underlayingObject = underlaying;
        }

        protected void ValidateType(string typeName, StateManagementValue value, string err = default)
        {
            Guard.NotNull(value, nameof(value));
            Guard.NotNull(typeName, nameof(typeName));

            if (value.GetType().Name != typeName) throw new NotSupportedException($"Value {value} is not {typeName}.{(string.IsNullOrWhiteSpace(err) ? string.Empty : $" {err}")}");
        }
        protected T Cast<T>(StateManagementValue value, string err = default) where T : StateManagementValue
        {
            Guard.NotNull(value, nameof(value));

            if (!(value is T t)) throw new NotSupportedException($"Value {value} is not {typeof(T).Name} and can not be casted to this type.{(string.IsNullOrWhiteSpace(err) ? string.Empty : $" {err}")}");

            return t;
        }

        public virtual StateManagementValue Index(StateManagementValue value) => throw new NotSupportedException($"Operator {nameof(Index)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Pop() => throw new NotSupportedException($"Operator {nameof(Pop)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Push(StateManagementValue value) => throw new NotSupportedException($"Operator {nameof(Push)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Clear() => throw new NotSupportedException($"Operator {nameof(Clear)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Length() => throw new NotSupportedException($"Operator {nameof(Length)} is not supported operator for type {GetType()}");
        public virtual bool Contains(StateManagementValue value) => throw new NotSupportedException($"Operator {nameof(Contains)} is not supported operator for type {GetType()}");

        public virtual StateManagementValue Format(List<StateManagementValue> parameters) => throw new NotSupportedException($"Operator {nameof(Format)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Substring(StateManagementValue start, StateManagementValue length) => throw new NotSupportedException($"Operator {nameof(Substring)} is not supported operator for type {GetType()}");
        public virtual bool CurrentUserInGroup(IList<string> currentUsersGroup) => throw new NotSupportedException($"Operator {nameof(CurrentUserInGroup)} is not supported operator for type {GetType()}");
        public virtual Task<bool> UserInGroupAsync(StateManagementValue value) => throw new NotSupportedException($"Operator {nameof(UserInGroupAsync)} is not supported operator for type {GetType()}");
        public virtual Task<bool> IsGroupNotEmptyAsync() => throw new NotSupportedException($"Operator {nameof(IsPermissionGroupEmptyAsync)} is not supported operator for type {GetType()}");
        public virtual Task<bool> IsPermissionGroupEmptyAsync() => throw new NotSupportedException($"Operator {nameof(IsPermissionGroupEmptyAsync)} is not supported operator for type {GetType()}");
        public virtual bool NotEmpty() => throw new NotSupportedException($"Operator {nameof(NotEmpty)} is not supported operator for type {GetType()}");
        public virtual bool Empty() => throw new NotSupportedException($"Operator {nameof(Empty)} is not supported operator for type {GetType()}");
        public virtual bool IsNull() => false;
        public virtual bool IsNotNull() => true;

        public virtual StateManagementValue AddYears(StateManagementValue days) => throw new NotSupportedException($"Operator {nameof(AddYears)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue AddMonths(StateManagementValue days) => throw new NotSupportedException($"Operator {nameof(AddMonths)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue AddDays(StateManagementValue days) => throw new NotSupportedException($"Operator {nameof(AddDays)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue AddHours(StateManagementValue days) => throw new NotSupportedException($"Operator {nameof(AddHours)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue AddMinutes(StateManagementValue days) => throw new NotSupportedException($"Operator {nameof(AddMinutes)} is not supported operator for type {GetType()}");

        public static bool operator ==(StateManagementValue left, StateManagementValue right) => left.ValueEquals(right);
        public static bool operator !=(StateManagementValue left, StateManagementValue right) => left.ValueNotEquals(right);
        public static bool operator <(StateManagementValue left, StateManagementValue right) => left.LessThen(right);
        public static bool operator >(StateManagementValue left, StateManagementValue right) => left.GreaterThen(right);
        public static bool operator <=(StateManagementValue left, StateManagementValue right) => left.LessThenOrEqual(right);
        public static bool operator >=(StateManagementValue left, StateManagementValue right) => left.GreaterThenOrEqual(right);

        public static StateManagementValue operator +(StateManagementValue left, StateManagementValue right) => left.Add(right);
        public static StateManagementValue operator -(StateManagementValue left, StateManagementValue right) => left.Sub(right);
        public static StateManagementValue operator *(StateManagementValue left, StateManagementValue right) => left.Mul(right);
        public static StateManagementValue operator /(StateManagementValue left, StateManagementValue right) => left.Div(right);
        public static StateManagementValue operator !(StateManagementValue left) => left.Neg();
        
        public virtual bool ValueEquals(StateManagementValue right) => throw new NotSupportedException($"Operator {nameof(ValueEquals)} is not supported operator for type {GetType()}");
        public virtual bool ValueNotEquals(StateManagementValue right) => throw new NotSupportedException($"Operator {nameof(ValueNotEquals)} is not supported operator for type {GetType()}");
        public virtual bool LessThenOrEqual(StateManagementValue right) => throw new NotSupportedException($"Operator {nameof(LessThenOrEqual)} is not supported operator for type {GetType()}");
        public virtual bool GreaterThenOrEqual(StateManagementValue right) => throw new NotSupportedException($"Operator {nameof(GreaterThenOrEqual)} is not supported operator for type {GetType()}");
        public virtual bool LessThen(StateManagementValue stateManagementValue) => throw new NotSupportedException($"Operator {nameof(LessThen)} is not supported operator for type {GetType()}");

        public virtual bool GreaterThen(StateManagementValue stateManagementValue) => throw new NotSupportedException($"Operator {nameof(GreaterThen)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Add(StateManagementValue stateManagementValue) => throw new NotSupportedException($"Operator {nameof(Add)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Sub(StateManagementValue stateManagementValue) => throw new NotSupportedException($"Operator {nameof(Sub)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Mul(StateManagementValue stateManagementValue) => throw new NotSupportedException($"Operator {nameof(Mul)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Div(StateManagementValue stateManagementValue) => throw new NotSupportedException($"Operator {nameof(Div)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Neg() => throw new NotSupportedException($"Operator {nameof(Neg)} is not supported operator for type {GetType()}");
        public virtual StateManagementValue Abs() => throw new NotSupportedException($"Operator {nameof(Abs)} is not supported operator for type {GetType()}");
        public override bool Equals(object obj)
        {
            return obj is StateManagementValue value &&
                   EqualityComparer<object>.Default.Equals(_underlayingObject, value._underlayingObject);
        }

        public override int GetHashCode()
        {
            return -351767848 + EqualityComparer<object>.Default.GetHashCode(_underlayingObject);
        }

        public static IList<StateManagementValueData> DeserializeCollectionProperties(string propertyCollectionXml)
        {
            if (string.IsNullOrWhiteSpace(propertyCollectionXml)) return new List<StateManagementValueData> { };
            
            byte[] data = System.Text.Encoding.Unicode.GetBytes(propertyCollectionXml);
                        
            var ser = new DataContractSerializer(typeof(List<StateManagementValueData>));
                        
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(ms, System.Text.Encoding.Unicode, new XmlDictionaryReaderQuotas { }, null))
                {
                    return (List<StateManagementValueData>)ser.ReadObject(reader);
                }
            }
        }

        public static string SerializeCollectionProperties(List<StateManagementValueData> properties) 
        {
            Guard.NotNull(properties, nameof(properties));

            var ser = new DataContractSerializer(typeof(List<StateManagementValueData>));

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlDictionaryWriter.CreateTextWriter(ms, System.Text.Encoding.Unicode))
                {
                    ser.WriteObject(writer, properties);
                    writer.Close();
                    return System.Text.Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }

        public string Serialize()
        {
            var ser = new DataContractSerializer(typeof(StateManagementValue), GetAllStatemanagementValueTypes());

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlDictionaryWriter.CreateTextWriter(ms, System.Text.Encoding.Unicode))
                {
                    ser.WriteObject(writer, this);
                    writer.Close();
                    return System.Text.Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }

        public static StateManagementValue Deserialize(string value, string valueType)
        {
            Guard.StringNotNullOrWhiteSpace(value, nameof(value));
            Guard.StringNotNullOrWhiteSpace(valueType, nameof(valueType));

            byte[] data = System.Text.Encoding.Unicode.GetBytes(value);

            var type = Type.GetType(valueType);

            var ser = new DataContractSerializer(typeof(StateManagementValue), GetAllStatemanagementValueTypes());

            Guard.NotNull(type, nameof(type), $"Cannot find type by type name : {valueType}");

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(ms, System.Text.Encoding.Unicode, new XmlDictionaryReaderQuotas { }, null))
                {
                    return (StateManagementValue)ser.ReadObject(reader);
                }
            }
        }

        protected string ToStringFormatter(string defaultString)
        {
            if (string.IsNullOrWhiteSpace(_formatter)) return defaultString;

            Match match = default;

            var matchedFormatter = Formatters.FirstOrDefault(f => { match = f.regex.Match(_formatter); return match.Success; });

            return !Equals(null, match) && match.Success
                ? matchedFormatter.evaluator(match.Value)
                : defaultString;
        }

        private static Type[] GetAllStatemanagementValueTypes() =>
            Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && typeof(StateManagementValue).IsAssignableFrom(t)).ToArray();
    }
}
