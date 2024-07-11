using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.Helpers
{
    public class NumberHasMaxDecimalsAttribute : ValidationAttribute
    {
        private readonly int numberDecimals;

        public NumberHasMaxDecimalsAttribute(int numberDecimals)
        {
            this.numberDecimals = numberDecimals;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            object instance = context.ObjectInstance;

            if (instance == null) return base.IsValid(value, context);

            if (value is not null && value is decimal decimalValue)
            {
                if (decimal.Round(decimalValue, numberDecimals) != decimalValue)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
