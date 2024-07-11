using RhDev.Common.Web.Core.Utils;
using System.Runtime.Serialization;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    public class StateManagementArrayValue : StateManagementValue   
    {
        [DataMember]
        private List<StateManagementValue> data;

        [DataMember]
        private string itemType;

        public List<StateManagementValue> Data => data;
        public string ItemType => itemType;
        public StateManagementArrayValue(List<StateManagementValue> data, object underlaying, string? itemTypeName = default) : base(underlaying)
        {
            if ((data is null || data.Count == 0) && string.IsNullOrWhiteSpace(itemTypeName)) throw new InvalidOperationException("Neither data nor itemtype has been defined for array");

            this.data = data;

            itemType = data is null || data.Count == 0 ? itemTypeName : data.First().GetType().Name;
        }

        public override StateManagementValue Length() => new StateManagementNumberValue(Data.Count);

        public override StateManagementValue Push(StateManagementValue value)
        {
            ValidateType(ItemType, value, $"Pushed value must be the same type as array collection item types");

            Data.Add(value);

            return this;
        }

        public override StateManagementValue Pop()
        {
            this.data = data.Count > 0 ? this.data.GetRange(0, data.Count - 1) : this.data;

            return this;
        }

        public override StateManagementValue Index(StateManagementValue value)
        {
            var index = Cast<StateManagementNumberValue>(value, "Index operand must be number");

            var key = index.NumberValue;

            Guard.NumberInRange(key, 0, data.Count - 1, nameof(key));

            return data[(int)key];
        }

        public override StateManagementValue Clear()
        {
            Data.Clear();

            return this;
        }

        public override bool Contains(StateManagementValue value) => data.Any(d => d.ValueEquals(value));

        public override bool Empty() => Data.Count == 0;
        public override bool NotEmpty() => !Empty();

        public override StateManagementValue Add(StateManagementValue stateManagementValue)
        {
            var array = Cast<StateManagementArrayValue>(stateManagementValue);

            if (ItemType != array.ItemType) throw new InvalidOperationException("Arrays item type mismatch");

            var mergedArray = new List<StateManagementValue>();
            mergedArray.AddRange(Data);
            mergedArray.AddRange(array.Data);

            return new StateManagementArrayValue(mergedArray, mergedArray);
        }
    }
}
