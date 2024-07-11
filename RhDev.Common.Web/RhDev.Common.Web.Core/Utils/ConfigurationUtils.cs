using RhDev.Common.Web.Core.Configuration;
using System.Linq.Expressions;
using System.Reflection;

namespace RhDev.Common.Web.Core.Utils
{
    public class ConfigurationUtils
    {
        public static string GetPathConfigurationProperty<T>(Expression<Func<T, object>> configPropertyEvaluator) where T : IApplicationConfigurationSection
        {
            Guard.NotNull(configPropertyEvaluator, nameof(configPropertyEvaluator));

            var memberExpression = configPropertyEvaluator.Body as MemberExpression;

            if (memberExpression is null)
            {
                if (configPropertyEvaluator.Body is UnaryExpression ue)
                {
                    memberExpression = ue.Operand as MemberExpression;
                }
            }

            if (memberExpression is null) throw new NotSupportedException("Body is not a MemberExpression");

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo is null) throw new NotSupportedException("Only properties are allowed as config property evaluators" + memberExpression.Member);

            var propertyType = propertyInfo.DeclaringType;

            Guard.NotNull(propertyType, nameof(propertyType), $"Property type is null");

            var propertyObject = Activator.CreateInstance(propertyType);

            if (propertyObject is IApplicationConfigurationSection configurationSection) return $"{configurationSection.Path}:{propertyInfo.Name}";

            throw new InvalidOperationException("Property containing object is not IApplicationConfigurationSection");
        }

        public static (string path, Type propertyType, MemberInfo mi) GetPathConfigurationPropertyUsingExpressionWalker<T>(Expression<Func<T, object>> configPropertyEvaluator) where T : IApplicationConfigurationSection
        {
            Guard.NotNull(configPropertyEvaluator);
            List<string> paths = new();
            Type propertyType = default!;
            MemberInfo mi = default;

            WalkLambdaPropertyExpression(configPropertyEvaluator.Body, ref paths, ref propertyType, ref mi);

            paths.Reverse();

            return (string.Join(":", paths), propertyType, mi);
        }

        private static void WalkLambdaPropertyExpression(Expression expression, ref List<string> elements, ref Type propertyType, ref MemberInfo mi)
        {
            if (expression == null)
                return;

            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                    {
                        if (expression is UnaryExpression ue)
                        {
                            var operandMemberExpression = ue.Operand as MemberExpression;

                            WalkLambdaPropertyExpression(operandMemberExpression!, ref elements, ref propertyType, ref mi);
                        }

                        break;
                    }

                case ExpressionType.Parameter:
                    {
                        var parameter = expression as ParameterExpression;

                        if (typeof(IApplicationConfigurationSection).IsAssignableFrom(parameter!.Type))
                        {
                            var config = (IApplicationConfigurationSection)Activator.CreateInstance(parameter.Type)!;

                            elements.Add(config.Path);
                        }

                        if (propertyType is null) propertyType = parameter.Type;

                        break;
                    }
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;

                    var memberName = memberExpression.Member.Name;
                    elements.Add(memberName);

                    if (propertyType is null) propertyType = memberExpression.Type;

                    if (mi is null) mi = memberExpression.Member;

                    WalkLambdaPropertyExpression(memberExpression.Expression, ref elements, ref propertyType, ref mi);
                    break;


                case ExpressionType.Call:
                    if (expression is MethodCallExpression callExpression
                        && callExpression.Method.IsSpecialName
                        && callExpression.Method.Name == "get_Item")
                    {
                        var args = callExpression.Arguments;
                        if (args.Count == 1)
                        {
                            var first = args.First();
                            if (first is ConstantExpression ce)
                            {
                                var indexerValue = ce.Value;

                                elements.Add(indexerValue.ToString());
                            }

                            if (first is MemberExpression fieldExpression)
                            {
                                var fieldName = fieldExpression.Member.Name;
                                var constantExpression = fieldExpression.Expression as ConstantExpression;
                                var value = constantExpression.Value;
                                var field = value.GetType().GetField(fieldName);
                                var fieldValue = field.GetValue(value);

                                elements.Add(fieldValue.ToString());
                            }

                            if (propertyType is null) propertyType = callExpression.Method.ReturnType;
                        }
                        else throw new InvalidOperationException("Multiple arguments, expression not supported");

                        var calledOn = callExpression.Object;

                        WalkLambdaPropertyExpression(calledOn!, ref elements, ref propertyType, ref mi);
                    }
                    else throw new InvalidOperationException("Expression is not supported");
                    break;

                default:
                    throw new InvalidOperationException("Expression is not supported");
            }
        }
    }
}
