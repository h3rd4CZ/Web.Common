using RhDev.Common.Web.Core.Utils;
using System.Security.Claims;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        public static List<string> GetAllRoles(this ClaimsPrincipal claimsPrincipal)
        {
            Guard.NotNull(claimsPrincipal, nameof(claimsPrincipal));

            var identity = claimsPrincipal?.Identity as ClaimsIdentity;

            Guard.NotNull(identity, nameof(identity));

            var identityClaims = identity.Claims;

            return identityClaims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();
        }
    }
}
