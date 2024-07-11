using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.Helpers
{
    public class DecimalValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null || value.ToString().Length == 0)
            {
                return ValidationResult.Success;
            }

            return !decimal.TryParse(value.ToString(), out decimal d) ? new ValidationResult(ErrorMessage) : ValidationResult.Success;
        }
    }
}
