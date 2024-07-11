using Microsoft.AspNetCore.Identity;

namespace RhDev.Common.Web.Core.Security
{
    public class IdentitySettings
    {
        public const int PWD_OPTIONS_MINLENGTH = 8;
        public const int PWD_2FA_OPTIONS_MINLENGTH = 6;
        public const int PWD_2FA_OPTIONS_MAXLENGTH = 6;


        public static PasswordOptions PasswordOptions = new PasswordOptions
        {
            RequireDigit = true,
            RequiredLength = PWD_OPTIONS_MINLENGTH,
            RequireLowercase = true,
            RequireNonAlphanumeric = false,
            RequireUppercase = true
        };
    }
}
