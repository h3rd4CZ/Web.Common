using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Security
{
    public class CurrentIdentity : IDisposable
    {
        private static AsyncLocal<Guid?> _currentIdentity = new AsyncLocal<Guid?>();
        private static AsyncLocal<string?> _currentApp = new AsyncLocal<string?>();

        public CurrentIdentity(Guid usertId)
        {
            _currentIdentity.Value = usertId;
        }

        public CurrentIdentity(Guid usertId, string appId)
        {
            _currentIdentity.Value = usertId;
            _currentApp.Value = appId;
        }

        public static Guid? IdentityValue => _currentIdentity?.Value;
        public static string? AppValue => _currentApp?.Value;

        public void Dispose()
        {
            _currentIdentity.Value = default;
        }
    }
}
