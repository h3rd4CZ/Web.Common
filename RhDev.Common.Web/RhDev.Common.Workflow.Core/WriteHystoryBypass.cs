using System;

namespace RhDev.Common.Workflow
{
    public class WriteHystoryBypass : IDisposable
    {
        private static AsyncLocal<bool?> _isActive = new AsyncLocal<bool?>();

        public WriteHystoryBypass()
        {
            _isActive.Value = true;
        }

        public static bool? IsActive => _isActive.Value;

        public void Dispose() => _isActive.Value = default;
    }
}
