using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Actions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Actions
{
    public abstract class StateManagementStateFullActionBase
    {
        protected IPropertyEvaluator PropertyEvaluator { get; }
        protected IWorkflowPropertyManager WorkflowPropertyManager { get; }
        public IDictionary<string, string> ActionState { get; } = new Dictionary<string, string>();

        public const char PARAMS_DELIMITER = ',';
        public const char PARAMS_KEYVALEQUAL = ':';

        protected StateManagementStateFullActionBase(IPropertyEvaluator propertyEvaluator, IWorkflowPropertyManager workflowPropertyManager)
        {
            PropertyEvaluator = propertyEvaluator;
            WorkflowPropertyManager = workflowPropertyManager;
        }

        protected async Task<StateManagementValue> ExtractActionParameter(string parameterName, List<StateMachineActionParameter> parameters, StateTransitionEventArgs args, bool throWhenParamNotFound = true, bool throwWhenNull = true)
        {
            Guard.NotNull(parameters, nameof(parameters));

            var parameter = parameters.FirstOrDefault(p => p.Name == parameterName);

            if (throWhenParamNotFound)
                Guard.NotNull(parameter, nameof(parameter), $"Parameter {parameterName} was not found, please check Workflow definition file");

            if (Equals(null, parameter)) return new StateManagementNullValue();

            var evaluatedParameter = await EvaluateProperty(parameter.Operand, args);

            Guard.NotNull(evaluatedParameter, nameof(evaluatedParameter));

            if (evaluatedParameter is StateManagementNullValue && throwWhenNull) throw new InvalidOperationException($"Parameter name {parameterName} was evaluated as null");

            return evaluatedParameter;
        }

        protected async Task<StateManagementValue> EvaluateProperty(Operand operand, StateTransitionEventArgs args)
        {
            Guard.NotNull(operand, nameof(operand));
            Guard.NotNull(args, nameof(args));

            return await PropertyEvaluator.EvaluateAsync(operand, args);
        }

        protected void UpdateState(string key, string value)
        {
            Guard.StringNotNullOrWhiteSpace(key, nameof(key));
            Guard.StringNotNullOrWhiteSpace(value, nameof(value));

            ActionState[key] = value;
        }

        protected IDictionary<string, string> DeserializeParams(string paramsString)
        {
            var res = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(paramsString)) return res;


            var paramsVal = paramsString.Split(PARAMS_DELIMITER);

            paramsVal.ToList().ForEach(p =>
            {
                var k = p.Split(PARAMS_KEYVALEQUAL)[0];
                var v = p.Split(PARAMS_KEYVALEQUAL)[1];
                res.Add(k, v);
            });

            return res;
        }

        protected string GetParam(IDictionary<string, string> data, string key, bool throwIfNotExist)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string val;

            if (!data.TryGetValue(key, out val) && throwIfNotExist)
                throw new InvalidOperationException($"Value with key {key} was not found in params");


            return val;
        }

        protected string EncodeDateString(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return dateString;

            return dateString.Replace(":", "@");
        }

        protected string DecodeDateString(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return dateString;

            return dateString.Replace("@", ":");
        }

        private string EncodePermissionMember(string member)
        {
            if (string.IsNullOrEmpty(member)) return member;

            return member.Replace(":", "@");
        }

        private string DecodePermissionMember(string member)
        {
            if (string.IsNullOrEmpty(member)) return member;

            return member.Replace("@", ":");
        }

    }
}
