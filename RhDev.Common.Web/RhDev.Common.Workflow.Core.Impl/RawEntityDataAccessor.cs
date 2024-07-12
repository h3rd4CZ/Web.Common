using JasperFx.Core.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Extensions;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;
using System.Reflection;

namespace RhDev.Common.Workflow.DataAccess.SharePoint.Online
{
    public class RawEntityDataAccessor : IRawEntityDataAccessor
    {
        List<Type> supportedNumericTypes = new List<Type>()
        {
               typeof(byte),
               typeof(sbyte),
               typeof(short),
               typeof(ushort),
               typeof(int),
               typeof(uint),
               typeof(long),
               typeof(ulong),
               typeof(float),
               typeof(double),
               typeof(decimal),

               typeof(Nullable<byte>),
               typeof(Nullable<sbyte>),
               typeof(Nullable<short>),
               typeof(Nullable<ushort>),
               typeof(Nullable<int>),
               typeof(Nullable<uint>),
               typeof(Nullable<long>),
               typeof(Nullable<ulong>),
               typeof(Nullable<float>),
               typeof(Nullable<double>),
               typeof(Nullable<decimal>)
        };
        bool IsSupportedNumericType(Type propertyType) => supportedNumericTypes.Any(t => t == propertyType);

        private readonly IUserInfoValueEvaluator userInfoValueEvaluator;
        private readonly IDynamicDataStoreRepository<DbContext> dynamicDataStore;

        public RawEntityDataAccessor(
            IUserInfoValueEvaluator userInfoValueEvaluator, 
            IDynamicDataStoreRepository<DbContext> dynamicDataStore)
        {
            this.userInfoValueEvaluator = userInfoValueEvaluator;
            this.dynamicDataStore = dynamicDataStore;
        }


        public async Task SetEntityFieldValuesAndUpdateAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, IDictionary<string, StateManagementValue> values)
        {
            Guard.NotNull(values, nameof(values));
            ValidateDocumentIdentifier(workflowDocumentIdentifier);

            if (values.Count == 0) return;

            IWorkflowDocument dataEntity = await ReadDocument(workflowDocumentIdentifier);

            foreach (var val in values)
            {
                var propertyName = val.Key;
                var value = val.Value;

                await SetValue(dataEntity, propertyName, value);
            }

            await dynamicDataStore.UpdateEntityAsync(dataEntity);
        }
                
        public async Task<StateManagementValue> GetEntityFieldValueAsync(WorkflowDocumentIdentifier workflowDocumentIdentifier, string propertyName)
        {
            Guard.StringNotNullOrWhiteSpace(propertyName);
            ValidateDocumentIdentifier(workflowDocumentIdentifier);

            IWorkflowDocument dataEntity = await ReadDocument(workflowDocumentIdentifier);

            return await GetValue( dataEntity, propertyName, workflowDocumentIdentifier.SectionDesignation);
        }

        async Task<StateManagementValue> GetValue(IWorkflowDocument item, string propertyName, SectionDesignation sectionDesignation)
        {
            Guard.NotNull(item);
            Guard.StringNotNullOrWhiteSpace(propertyName);

            var isNestedObjectAccessor = ObjectAccessor.IsNestedObjectAccessor(propertyName);

            var property = ObjectAccessor.GetPublicPropertyValue(item, propertyName);

            if(property.propertyValue is null && isNestedObjectAccessor) return new StateManagementNullValue();

            Guard.NotNull(property.propertyType);
            Guard.NotNull(property.pi);

            var value = property.propertyValue;
            var propertyType = property.propertyType;   
            var pi = property.pi;
                        
            if ((Nullable.GetUnderlyingType(propertyType) is not null && value == default) || 
                !propertyType.IsValueType && value == default) return new StateManagementNullValue();

            if (propertyType == typeof(string))
            {
                if (IsSupportedUserFieldType(pi))
                {
                    return await userInfoValueEvaluator.EvaluateAsUserAsync(value!, sectionDesignation);
                }
                else
                {
                    if (IsApplicationUserFieldType(pi, item))
                    {
                        return await userInfoValueEvaluator.EvaluateAsUserAsync(value!, sectionDesignation);
                    }
                    else
                    {
                        return new StateManagementTextValue((string)value!);
                    }
                }
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(Nullable<DateTime>))
            {
                var dateTime = Convert.ToDateTime(value);

                return new StateManagementDateTimeValue(dateTime);
            }
            else if (propertyType == typeof(bool) || propertyType == typeof(Nullable<bool>))
            {
                var boolean = Convert.ToBoolean(value);

                return new StateManagementBooleanValue(boolean);
            }
            else if (IsSupportedNumericType(propertyType))
            {
                var doubleNumber = ParseToDouble(value!);

                return new StateManagementNumberValue(doubleNumber);
            }
            else
            {
                throw new InvalidOperationException($"{propertyType} is not supported type");
            }
        }

        async Task SetValue(IWorkflowDocument item, string propertyName, StateManagementValue value)
        {
            Guard.NotNull(item);
            Guard.StringNotNullOrWhiteSpace(propertyName);
            Guard.NotNull(value);

            var itemType = item.GetType();

            var pi = itemType
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;

            Guard.NotNull(pi, nameof(pi), $"Property name wes not found by its property name : {propertyName}");

            var propertyType = pi.PropertyType;

            Guard.NotNull(pi, nameof(pi), $"Property name : {propertyName} was not found");

            if (value.IsNullValue)
            {
                pi.SetValue(item, default);

                return;
            }

            if (propertyType == typeof(string))
            {
                if (IsSupportedUserFieldType(pi))
                {
                    var user = CheckCompatibilityType<StateManagementUserValue>(value);
                    pi.SetValue(item, user.Id);
                }
                else
                {
                    if (value is StateManagementTextValue text)
                    {
                        pi.SetValue(item, text.TextValue);
                    }
                    else if (value is StateManagementUserValue user && !user.IsPermissionGroup)
                    {
                        pi.SetValue(item, user.Id);
                    }
                    else throw new InvalidOperationException($"For string property type is only allowed text or user values with user identifier only");
                }
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(Nullable<DateTime>))
            {
                var dateTime = CheckCompatibilityType<StateManagementDateTimeValue>(value);

                pi.SetValue(item, dateTime.DateTime);
            }
            else if (propertyType.IsEnum())
            {
                var enumNumber = CheckCompatibilityType<StateManagementNumberValue>(value);

                object enumValue = Enum.ToObject(propertyType, (int)enumNumber.NumberValue); 

                pi.SetValue(item, enumValue);
            }
            else if (propertyType == typeof(bool) || propertyType == typeof(Nullable<bool>))
            {
                var boolean = CheckCompatibilityType<StateManagementBooleanValue>(value);

                pi.SetValue(item, boolean.BooleanValue);
            }
            else if (IsSupportedNumericType(propertyType))
            {
                var number = CheckCompatibilityType<StateManagementNumberValue>(value);
                var doubleNumber = number.NumberValue;

                object parsedNumber = ParseDoubleAs(propertyType, doubleNumber);
                pi.SetValue(item, parsedNumber);
            }
            else
            {
                throw new InvalidOperationException($"{propertyType} is not supported type");
            }
        }

        T CheckCompatibilityType<T>(StateManagementValue value) where T : StateManagementValue
        {
            Guard.NotNull(value, nameof(value));

            if (value is T t) return t;

            throw new NotSupportedException($"Value {value} is not of required type : {typeof(T)}");
        }

        void CheckCompatibilityType(StateManagementValue value, params Type[] typesToCheck)
        {
            Guard.NotNull(value, nameof(value));
            Guard.CollectionNotNullAndNotEmpty(typesToCheck, nameof(typesToCheck));

            foreach (var type in typesToCheck)
                if (value.GetType() == type) return;

            throw new InvalidOperationException($"Value is not of one of the folowing types : {string.Join(",", typesToCheck.Select(t => t.Name))}");
        }

        void CheckArrayItemType(StateManagementArrayValue array, Type itemType)
        {
            Guard.NotNull(array, nameof(array));
            Guard.NotNull(itemType, nameof(itemType));

            if (array.ItemType != itemType.Name)
                throw new InvalidOperationException($"Array value item type is not of type : {itemType.Name}");
        }
                
        object ParseDoubleAs(Type toType, double value)
        {
            if (toType == typeof(byte) || toType == typeof(Nullable<byte>))
                return Convert.ChangeType(value, typeof(byte));
            else if (toType == typeof(sbyte) || toType == typeof(Nullable<sbyte>))
                return Convert.ChangeType(value, typeof(sbyte));
            else if (toType == typeof(short) || toType == typeof(Nullable<short>))
                return Convert.ChangeType(value, typeof(short));
            else if (toType == typeof(ushort) || toType == typeof(Nullable<ushort>))
                return Convert.ChangeType(value, typeof(ushort));
            else if (toType == typeof(int) || toType == typeof(Nullable<int>))
                return Convert.ChangeType(value, typeof(int));
            else if (toType == typeof(uint) || toType == typeof(Nullable<uint>))
                return Convert.ChangeType(value, typeof(uint));
            else if (toType == typeof(long) || toType == typeof(Nullable<long>))
                return Convert.ChangeType(value, typeof(long));
            else if (toType == typeof(ulong) || toType == typeof(Nullable<ulong>))
                return Convert.ChangeType(value, typeof(ulong));
            else if (toType == typeof(float) || toType == typeof(Nullable<float>))
                return Convert.ChangeType(value, typeof(float));
            else if (toType == typeof(double) || toType == typeof(Nullable<double>))
                return Convert.ChangeType(value, typeof(double));
            else if (toType == typeof(decimal) || toType == typeof(Nullable<decimal>))
                return Convert.ChangeType(value, typeof(decimal));
            else
                throw new InvalidOperationException("Type not supported.");
        }

        double ParseToDouble(object value)
        {
            object convertedValue = Convert.ChangeType(value, typeof(double));

            return (double)convertedValue;
        }

        bool IsSupportedUserFieldType(PropertyInfo pi)
        {
            return pi.HasAttribute<WorkflowUserFieldAttribute>();
        }

        bool IsApplicationUserFieldType(PropertyInfo pi, object propertyObject)
        {
            if(pi.PropertyType == typeof(string))
            {
                var piName = pi.Name;

                if (piName.EndsWith("Id") && piName.Length > 2)
                {
                    var correspondingUserObjectPropertyName = piName.Substring(0, piName.Length - 2);

                    var matchedProperty = propertyObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                       .FirstOrDefault(pi => pi.Name == correspondingUserObjectPropertyName);

                    if(matchedProperty is not null)
                    {
                        return typeof(IdentityUser).IsAssignableFrom(matchedProperty.PropertyType);
                    }

                    return false;
                }
            }

            return false;
        }

        async Task<IWorkflowDocument> ReadDocument(WorkflowDocumentIdentifier workflowDocumentIdentifier)
        {
            var identificator = workflowDocumentIdentifier.Identificator;
                        
            var dataEntity = await dynamicDataStore
                .ReadEntityByIdAsync<IWorkflowDocument>(
                identificator.typeName!, 
                identificator.entityId,
                GetExplicitIncludes(identificator.typeName));

            return dataEntity;

            string[] GetExplicitIncludes(string typeName)
            {
                var entityType = (IWorkflowDocument)Activator.CreateInstance(Type.GetType(typeName, false)!)!;

                return entityType.ExplicitIncludes;
            }
        }

        void ValidateDocumentIdentifier(WorkflowDocumentIdentifier workflowDocumentIdentifier)
        {
            Guard.NotNull(workflowDocumentIdentifier);
            Guard.NotNull(workflowDocumentIdentifier.Identificator);
            Guard.NotNull(workflowDocumentIdentifier.SectionDesignation);
        }
    }
}
