using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Workflow.PropertyModel.Properties;

namespace RhDev.Common.Workflow
{
    public interface IConditionEvaluator : IAutoregisteredService
    {
        Task<bool> EvaluateAsync(StateTransitionEventArgs args, ConditionExpression conditionRoot);
        Task<List<StateManagementUserValue>> EvaluateAggregatedApproval(List<StateManagementValue> operands);
    }
}
