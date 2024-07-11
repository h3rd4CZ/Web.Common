using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Security;
using RhDev.Common.Web.Core.Utils;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class StringExtensions
    {
        private static string DomainMapper(Match match)
        {
            // Use IdnMapping class to convert Unicode domain names.
            var idn = new IdnMapping();

            // Pull out and process domain name (throws ArgumentException on invalid)
            var domainName = idn.GetAscii(match.Groups[2].Value);

            return match.Groups[1].Value + domainName;
        }
               
        
        public static string EncryptPlainString(this string plain)
        {
            return StringEncryptor.Encrypt(plain);
        }

        public static bool IsMajorVersion(this string version)
        {
            Guard.StringNotNullOrWhiteSpace(version, nameof(version));

            return version.EndsWith(".0");
        }

        public static string GetLastMajorVersion(this string version)
        {
            Guard.StringNotNullOrWhiteSpace(version, nameof(version));

            var majorMinor = version.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            Guard.CollectionHasExactlyNumberElements(majorMinor, 2, nameof(majorMinor), $"Version {version} is invalid version");

            return $"{majorMinor[0]}.0";
        }

        public static bool IsValidExternalLogin(this string login)
        {
            return login.IsValidEmailAddress();
        }

        public static bool IsValidPhoneNumber(this string number)
        {
            //TODO specific validation for SMS gate
            return true;
        }

        public static bool IsValidEmailAddress(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

        }


        public static string TrimLoginDomain(this string login)
        {
            if (string.IsNullOrEmpty(login)) throw new InvalidOperationException("String is null or empty");

            string[] data = login.Split('\\');
            if (data.Count() > 1) return data[1];
            return login;
        }

        public static string Base64Encoded(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string ClaimLess(this string str)
        {
            Guard.NotNull(str, nameof(str));

            return str.Split(new[] { "|" }, StringSplitOptions.None)
                .Last();
        }

        public static string ToMd5Fingerprint(this string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s.ToCharArray());
            var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);

            // concat the hash bytes into one long string
            return hash.Aggregate(new StringBuilder(32),
                (sb, b) => sb.Append(b.ToString("X2")))
                .ToString();
        }

        public static decimal GetDecimal(this string s)
        {
            if (decimal.TryParse(s.Replace(",", "."), out decimal result)) return result;

            if (decimal.TryParse(s.Replace(".", ","), out decimal result2)) return result2;

            return default;
        }

        public static int GetInt(this string s)
        {
            if (int.TryParse(s, out int result)) return result;

            return default;
        }

        public static DateOnly GetDate(this string s, string format = default)
        {
            if (DateOnly.TryParseExact(s, format ?? CommonConfiguration.DATE_FORMAT, default, DateTimeStyles.None, out DateOnly result)) return result;

            return default;
        }

        public static TimeOnly GetTime(this string s)
        {
            if (TimeOnly.TryParseExact(s, "HH:mm:ss", default, DateTimeStyles.None, out TimeOnly result)) return result;

            return default;
        }

        public static string RemoveDiacritics(this string str)
        {
            str = str.Normalize(NormalizationForm.FormD);
            var resultBuilder = new StringBuilder();

            foreach (char stringChar in str)
                if (CharUnicodeInfo.GetUnicodeCategory(stringChar) != UnicodeCategory.NonSpacingMark)
                    resultBuilder.Append(stringChar);

            return resultBuilder.ToString();
        }

        public static TimeSpan? AsTimeSpan(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            if (TimeSpan.TryParse(str, out TimeSpan span)) return span;

            throw new InvalidOperationException("No valid time span expression");
        }

        public static bool IsSecurityString(this MemberInfo mi)
        {
            return Attribute.IsDefined(mi, typeof(SafeStringAttribute));
        }
    }
}
