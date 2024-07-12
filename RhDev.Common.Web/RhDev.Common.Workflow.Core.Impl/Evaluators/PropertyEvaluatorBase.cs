using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Impl.Evaluators
{
    public abstract class PropertyEvaluatorBase
    {
        const string Iso8601StringFormat = "yyyy-MM-ddTHH:mm:ss";

        public const string TAXONOMYVALUETYPEDELIMITER = ";";
        public const string LOOKUPVALUETYPEDELIMITER = ";";
        public const string URLVALUEDELIMITER = ";#";
        public const string GROUPNAMEFETCHER_MULTIGROUP_DELIMITER = ",";
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;
        private readonly IUserInfoValueEvaluator userInfoValueEvaluator;
        protected IDictionary<string, Func<StateTransitionEventArgs, string, Task<string>>> _fetchers;

        public const string GROUPNAME_FETCHER = "groupname";
        public const string CURRENT_DATE_FETCHER = "currentdate";
        public const string CURRENT_ITEMID_FETCHER = "currentitemid";
        public const string CURRENT_ITEMTITLE_FETCHER = "currentitemtitle";
        public const string CURRENT_ITEMNAME_FETCHER = "currentitemname";
        public const string CURRENT_ITEMURL_FETCHER = "currentitemurl";
        public const string CURRENT_WORKLFOWTITLE_FETCHER = "currentworkflowtitle";
        public const string CURRENT_WORKLFOWUSER_FETCHER = "currentworkflowuserid";
        public const string WORKLFOWINITIATOR = "workflowinitiator";


        protected PropertyEvaluatorBase(
            IWorkflowInstanceRepository workflowInstanceRepository,
            IUserInfoValueEvaluator userInfoValueEvaluator)
        {
            _fetchers =
                new Dictionary<string, Func<StateTransitionEventArgs, string, Task<string>>>
                {
                    {CURRENT_DATE_FETCHER, CurrentDateFetcher},
                    {CURRENT_ITEMID_FETCHER, CurrentItemIdFetcher},
                    {CURRENT_ITEMURL_FETCHER, CurrentItemUrlFetcher},
                    {CURRENT_WORKLFOWTITLE_FETCHER, WorkflowNameFetcher},
                    {CURRENT_WORKLFOWUSER_FETCHER, CurrentWorkflowUserFetcher},
                    {WORKLFOWINITIATOR, WorkflowInitiatorFetcher},
                };
            this.workflowInstanceRepository = workflowInstanceRepository;
            this.userInfoValueEvaluator = userInfoValueEvaluator;
        }


        protected async Task<StateManagementValue> EvaluatePlainProperty(
            IPropertyEvaluator propertyEvaluator, Operand property, StateTransitionEventArgs args)
        {
            Guard.NotNull(property, nameof(property));
            Guard.NotNull(args, nameof(args));

            if (string.IsNullOrWhiteSpace(property.Fetcher))
            {
                return await CastTextPropertyAs(propertyEvaluator, property, property.Text, property.DataType, args);
            }

            if (!_fetchers.TryGetValue(property.Fetcher.ToLower(), out Func<StateTransitionEventArgs, string, Task<string>> fetcher))
                throw new InvalidOperationException($"Cannot find fetcher : {property.Fetcher}");

            var fetchedValue = await fetcher(args, property.Text);

            return await CastTextPropertyAs(propertyEvaluator, property, fetchedValue, property.DataType, args);
        }

        protected async Task<StateManagementValue> CastTextPropertyAs(
            IPropertyEvaluator propertyEvaluator, Operand operand, string propertyValue, WorkflowDataType parameterType,
            StateTransitionEventArgs args)
        {
            Guard.NotNull(args, nameof(args));
            Guard.NotNull(operand, nameof(operand));
            Guard.NotNull(propertyEvaluator, nameof(propertyEvaluator));

            bool isEmpty = string.IsNullOrWhiteSpace(propertyValue);

            switch (parameterType)
            {
                case WorkflowDataType.Null: return new StateManagementNullValue();
                case WorkflowDataType.Array:
                    {
                        var arrayOperands = operand.Operands ?? new List<Operand>();

                        var evaluatedArrayOperands = arrayOperands
                            .Select(async o => await propertyEvaluator.EvaluateAsync(o, args));

                        var evaluatedOperands = await Task.WhenAll(evaluatedArrayOperands);

                        if (evaluatedArrayOperands.Count() > 0) CheckHomogenousArray(evaluatedOperands.ToList());

                        return new StateManagementArrayValue(evaluatedOperands.ToList(), evaluatedArrayOperands.ToList(), operand.ArrayItemType);
                    }
                case WorkflowDataType.Boolean:
                    {
                        return isEmpty ?
                            new StateManagementNullValue() :
                            new StateManagementBooleanValue(Convert.ToBoolean(propertyValue));
                    }
                case WorkflowDataType.Url:
                    {
                        if (isEmpty) return new StateManagementNullValue();

                        var data = propertyValue.Split(new[] { URLVALUEDELIMITER }, StringSplitOptions.RemoveEmptyEntries);
                        Guard.NumberMin(data.Length, 1, nameof(data));

                        return new StateManagementUrlValue(data[0], data.Length > 1 ? data[1] : string.Empty);
                    }
                case WorkflowDataType.DateTime:
                    {
                        if (isEmpty) return new StateManagementNullValue();

                        if (DateTime.TryParseExact(propertyValue, Iso8601StringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                        {
                            return new StateManagementDateTimeValue(dateTime);
                        }

                        throw new InvalidOperationException($"{propertyValue} is not valid ISO 8601 datetime");
                    }

                case WorkflowDataType.Text:
                    {
                        return new StateManagementTextValue(string.IsNullOrWhiteSpace(propertyValue) ? string.Empty : propertyValue);
                    }
                case WorkflowDataType.User:
                    {
                        if (isEmpty) return new StateManagementNullValue();

                        var user = await userInfoValueEvaluator.EvaluateAsUserAsync((object)propertyValue, args.Designation);

                        Guard.NotNull(user, nameof(user), $"Can not evaluate {propertyValue} as User");

                        return user;
                    }
                case WorkflowDataType.Number:
                    {
                        if (isEmpty) return new StateManagementNullValue();

                        if (double.TryParse(propertyValue.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                            return new StateManagementNumberValue(d);

                        throw new InvalidOperationException($"{propertyValue} is not valid number");
                    }
                case WorkflowDataType.Unknown:
                    {
                        propertyValue = isEmpty ? string.Empty : propertyValue;

                        if (double.TryParse(propertyValue.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                            return new StateManagementNumberValue(d);
                        else if (bool.TryParse(propertyValue, out bool result)) return new StateManagementBooleanValue(result);
                        else
                        {
                            if (DateTime.TryParse(propertyValue, CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowWhiteSpaces, out DateTime dtm))
                            {
                                return new StateManagementDateTimeValue(dtm);
                            }

                            return new StateManagementTextValue(propertyValue);
                        }

                    }
                default: throw new InvalidOperationException($"An unknown parameter type detected : {parameterType}");
            }
        }

        private void CheckHomogenousArray(IEnumerable<StateManagementValue> evaluatedArrayOperands)
        {
            Guard.NotNull(evaluatedArrayOperands, nameof(evaluatedArrayOperands));

            Type type = default!;

            foreach (var item in evaluatedArrayOperands)
            {
                if (!Equals(null, type) && type != item.GetType())
                    throw new InvalidOperationException("Array must be homogenous collection structure, items must be of same type");

                type = item.GetType();
            }
        }

        private async Task<string> CurrentItemUrlFetcher(StateTransitionEventArgs args, string p)
        {
            throw new InvalidOperationException();
        }

        private async Task<string> WorkflowNameFetcher(StateTransitionEventArgs args, string p)
        {
            args.CheckValidWorkflowId();
            var wi = await workflowInstanceRepository.ReadByIdAsync(args.WorkflowId);
            return wi.Name;
        }

        private async Task<string> WorkflowInitiatorFetcher(StateTransitionEventArgs args, string p)
        {
            args.CheckValidWorkflowId();

            var wi = await workflowInstanceRepository.ReadByIdAsync(args.WorkflowId, include: new List<Expression<Func<WorkflowInstance, object>>> { w => w.InitiatorId });

            return wi.InitiatorId;
        }

        private async Task<string> CurrentItemIdFetcher(StateTransitionEventArgs args, string p) => await Task.FromResult(args.WorkflowDocumentIdentifier?.Identificator?.entityId.ToString()!);
        
        private async Task<string> CurrentDateFetcher(StateTransitionEventArgs args, string p)
        {
            return await Task.FromResult(DateTime.Now.ToString(Iso8601StringFormat));
        }

        private async Task<string> CurrentWorkflowUserFetcher(StateTransitionEventArgs args, string p)
        {
            return await Task.FromResult(args.UserId.ToString());
        }
    }
}
