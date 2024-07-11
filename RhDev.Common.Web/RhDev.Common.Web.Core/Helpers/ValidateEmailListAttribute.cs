using RhDev.Common.Web.Core.Extensions;
using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.Helpers
{
    public class ValidateEmailListAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is not null && value is IEnumerable<string> list)
            {
                return list.Any(l => !l.IsValidEmailAddress())
                   ? new ValidationResult(ErrorMessageString, new[] { context.MemberName })
                   : ValidationResult.Success;
            }
            else return new ValidationResult(ErrorMessageString);
        }
    }
}
