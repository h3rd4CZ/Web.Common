using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Evaluators
{
    public class ConditionEvaluator : OperandEvaluationEvaluatorBase, IConditionEvaluator
    {
        private readonly IPropertyEvaluator _propertyEvaluator;
        private string _userId;
        private IList<string> _userGroups;

        public ConditionEvaluator(
            IPropertyEvaluator propertyEvaluator
            )
        {
            _propertyEvaluator = propertyEvaluator;
        }
                
        public async Task<bool> EvaluateAsync(StateTransitionEventArgs args, ConditionExpression conditionRoot)
        {
            Guard.NotNull(args, nameof(args));

            if (Equals(null, conditionRoot)) return true;

            _userId = args.UserId;
            _userGroups = args.UserGroups;

            return await EvaluateCompositeCondition(conditionRoot, args);
        }

        public async Task<List<StateManagementUserValue>> EvaluateAggregatedApproval(List<StateManagementValue> operands)
        {
            ValidateOperandCount(2, operands.Count);
            var left = operands[0];
            var right = operands[1];

            var res = new List<StateManagementUserValue>();

            ValidateNotNullOperands(right);

            if (!(left is StateManagementArrayValue array && array.ItemType == typeof(StateManagementUserValue).Name))
                throw new InvalidOperationException($"Left operand must be array with items of type : {typeof(StateManagementUserValue).Name}");

            if (array.Data is null || right is StateManagementNullValue) return new();

            if (right is StateManagementUserValue permissionGroup && permissionGroup.IsPermissionGroup)
            {
                foreach (var a in array.Data)
                {
                    if (await a.UserInGroupAsync(permissionGroup)) res.Add((a as StateManagementUserValue)!);
                }
            }
            else if (right is StateManagementUserValue user && !user.IsPermissionGroup)
            {
                foreach (var a in array.Data) if (a.ValueEquals(user)) res.Add((a as StateManagementUserValue)!);
            }
            else throw new InvalidOperationException($"Right operand must be of type {typeof(StateManagementUserValue).Name}");

            return res;
        }

        private async Task<bool> EvaluateCompositeCondition(ConditionExpression conditionExpression, StateTransitionEventArgs args)
        {
            var isLogicalAnd = !Equals(null, conditionExpression.And) && conditionExpression.And.Count > 0;
            var isLogicalOr = !Equals(null, conditionExpression.Or) && conditionExpression.Or.Count > 0;
            var isLogicalNeg = !Equals(null, conditionExpression.Neg);

            var isLogical = isLogicalAnd || isLogicalOr || isLogicalNeg;

            if ((isLogicalAnd && isLogicalOr) || (isLogicalAnd && isLogicalNeg) || (isLogicalOr && isLogicalNeg)) 
                throw new InvalidOperationException("Condition expression is invalid, its not allowed to have AND or OR or NEG together, its allowed to have only one logical operand defined in one condition");

            var isElementaryCondition = conditionExpression.Operator != ComparativeOperator.Unknown;

            if (isLogical && isElementaryCondition) throw new InvalidOperationException("Condition expression is invalid, because elementary and logical operand is defined");

            if (!isLogical && !isElementaryCondition) return true;

            if (isElementaryCondition)
            {
                var conditionResult = await EvaluateCondition(new Condition { Operator = conditionExpression.Operator, Operands = conditionExpression.Operands }, args);

                return conditionResult;
            }
            else
            {
                if (isLogicalAnd)
                {
                    foreach (var andOperator in conditionExpression.And!)
                    {
                        var itemResult = await EvaluateCompositeCondition(andOperator, args);
                        if (!itemResult) return false;
                    }

                    return true;
                }
                if(isLogicalOr)
                {
                    foreach (var orOperator in conditionExpression.Or!)
                    {
                        var itemResult = await EvaluateCompositeCondition(orOperator, args);
                        if (itemResult) return true;
                    }

                    return false;
                }
                if(isLogicalNeg)
                {
                    var itemResult = await EvaluateCompositeCondition(conditionExpression.Neg!, args);
                    return !itemResult;
                }

                return false;
            }
        }

        private async Task<bool> EvaluateCondition(Condition condition, StateTransitionEventArgs args)
        {
            ValidateConditionValidity(condition);

            var evaluatedOperands = Equals(null, condition.Operands)
                ? new List<StateManagementValue> { }
                : (await Task.WhenAll(condition.Operands.Select(async o => await _propertyEvaluator.EvaluateAsync(o, args)))).ToList();

            var @operator = condition.Operator;

            return await EvaluateProperty(evaluatedOperands, @operator, args.Designation);
        }

        private void ValidateConditionValidity(Condition cond)
        {
            Guard.NotNull(cond, nameof(cond));

            Guard.CollectionNotNullAndNotEmpty(cond.Operands, nameof(cond.Operands), $"COndition must have at least one operand");

            if (cond.Operator == ComparativeOperator.Unknown) throw new InvalidOperationException("Condition operator is unknown");
        }

        private async Task<bool> EvaluateProperty(List<StateManagementValue> operands, ComparativeOperator opType, SectionDesignation designation)
        {
            switch (opType)
            {
                case ComparativeOperator.IsNull:
                    {
                        ValidateOperandCount(1, operands.Count);
                        var first = operands.First();
                        ValidateNotNullOperands(first);
                        return first.IsNull();
                    }
                case ComparativeOperator.IsNotNull:
                    {
                        ValidateOperandCount(1, operands.Count);
                        var first = operands.First();
                        ValidateNotNullOperands(first);
                        return first.IsNotNull();
                    }
                case ComparativeOperator.Empty:
                    {
                        ValidateOperandCount(1, operands.Count);
                        var first = operands.First();
                        ValidateNotNullOperands(first);
                        return first.Empty();
                    }
                case ComparativeOperator.NotEmpty:
                    {
                        ValidateOperandCount(1, operands.Count);
                        var first = operands.First();
                        ValidateNotNullOperands(first);
                        return first.NotEmpty();
                    }
                case ComparativeOperator.Equals:
                    {
                        return EvaluateEqualityExpression(operands);
                    }
                case ComparativeOperator.NotEquals:
                    {
                        return !EvaluateEqualityExpression(operands);
                    }
                case ComparativeOperator.Greater:
                    {
                        return EvaluateNumberExpression(operands, (l, r) => l > r);
                    }
                case ComparativeOperator.Less:
                    {
                        return EvaluateNumberExpression(operands, (l, r) => l < r);
                    }
                case ComparativeOperator.LessOrEqual:
                    {
                        return EvaluateNumberExpression(operands, (l, r) => l <= r);
                    }
                case ComparativeOperator.GreaterOrEqual:
                    {
                        return EvaluateNumberExpression(operands, (l, r) => l >= r);
                    }
                case ComparativeOperator.IsGroupEmpty:
                    {
                        return !await EvaluateGroupMembersNotEmpty(operands, designation);
                    }
                case ComparativeOperator.IsGroupNotEmpty:
                    {
                        return await EvaluateGroupMembersNotEmpty(operands, designation);
                    }
                case ComparativeOperator.UserInGroup:
                    {
                        ValidateOperandCount(2, operands.Count);
                        var left = operands[0];
                        var right = operands[1];
                        ValidateNotNullOperands(left, right);

                        return await left.UserInGroupAsync(right);

                    }
                case ComparativeOperator.CurrentUserInGroup:
                    {
                        ValidateOperandCount(1, operands.Count);
                        var operand = operands.First();
                        ValidateNotNullOperands(operand);

                        return operand.CurrentUserInGroup(_userGroups);
                    }
                case ComparativeOperator.CurrentUserIsSystem:
                    {
                        throw new NotImplementedException();
                    }
                case ComparativeOperator.Contains:
                    {
                        return EvaluateContainsCondition(operands);
                    }
                case ComparativeOperator.NotContains:
                    {
                        return !EvaluateContainsCondition(operands);
                    }
                case ComparativeOperator.AggregatedApproval:
                    {
                        var aggregatedResult =  await EvaluateAggregatedApproval(operands);

                        return aggregatedResult?.Count > 0;
                    }
                default:
                    throw new InvalidOperationException($"Unknown operator type");
            }
        }

        private bool EvaluateContainsCondition(List<StateManagementValue> operands)
        {
            ValidateOperandCount(2, operands.Count);
            var left = operands[0];
            var right = operands[1];
            ValidateNotNullOperands(operands[0], operands[1]);

            return left.Contains(right);
        }

        private async Task<bool> EvaluateGroupMembersNotEmpty(List<StateManagementValue> operands, SectionDesignation designation)
        {
            ValidateOperandCount(1, operands.Count);

            var operand = operands.First();

            return !(await operand.IsPermissionGroupEmptyAsync());
        }
                
        private bool EvaluateEqualityExpression(List<StateManagementValue> operands)
        {
            ValidateOperandCount(2, operands.Count);

            var left = operands[0];
            var right = operands[1];

            return left == right;
        }

        private bool EvaluateNumberExpression(List<StateManagementValue> operands, Func<StateManagementValue, StateManagementValue, bool> evaluator)
        {
            ValidateOperandCount(2, operands.Count);
            var left = operands[0];
            var right = operands[1];

            ValidateNotNullOperands(left, right);

            return evaluator(left, right);
        }
    }
}
