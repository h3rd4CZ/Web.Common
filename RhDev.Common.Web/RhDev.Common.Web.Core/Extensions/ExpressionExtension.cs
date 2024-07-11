using RhDev.Common.Web.Core.Utils;
using System.Linq.Expressions;
using System.Reflection;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class ExpressionExtension
    {
        public static void SetProperty<T, B>(this Expression<Func<T, B>> propertySelector, T target, B value, Func<T, object> targetElement = default)
        {
            Guard.NotNull(target, nameof(target));

            var setOn = targetElement is not null ? targetElement(target) : target;

            SetObjectPropertyInternal(setOn, propertySelector, value);
        }

        public static PropertyInfo GetObjectPropertyInfo<T, B>(this Expression<Func<T, B>> propertySelector)
        {
            if (propertySelector == null) throw new ArgumentNullException("propertySelector");

            var memberExpression = propertySelector.Body as MemberExpression;
                        
            if (memberExpression is null)
            {
                if (propertySelector.Body is UnaryExpression ue)
                {
                    memberExpression = ue.Operand as MemberExpression;
                }

                if (propertySelector.Body is MethodCallExpression) throw new InvalidOperationException("Cant get property info of method call expression");
            }
                                    
            if (memberExpression == null) throw new NotSupportedException("Cannot recognize property.");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null) throw new NotSupportedException("You can select property only. Currently, selected member is: " + memberExpression.Member);

            return propertyInfo;
        }

        public static void SetObjectProperty<T, B>(this Expression<Func<T, B>> propertySelector, object value, object target)
            => SetObjectPropertyInternal(target, propertySelector, value);



        private static void SetObjectPropertyInternal<T, B>(object target, Expression<Func<T, B>> propertySelector, object value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (propertySelector == null)
            {
                throw new ArgumentNullException("propertySelector");
            }

            var memberExpression = propertySelector.Body as MemberExpression;

            if (memberExpression is null)
            {
                if (propertySelector.Body is UnaryExpression ue)
                {
                    memberExpression = ue.Operand as MemberExpression;
                }
            }

            if (memberExpression == null) throw new NotSupportedException("Cannot recognize property.");

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo is null) throw new NotSupportedException("You can select property only. Currently, selected member is: " + memberExpression.Member);

            propertyInfo.SetValue(target, value);
        }
    }
}
