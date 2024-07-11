using System;

namespace RhDev.Common.Workflow
{
    public class StateManagementConditionBypass : IDisposable
    {
        private static AsyncLocal<bool?> _isActive = new AsyncLocal<bool?>();

        public StateManagementConditionBypass()
        {
            _isActive.Value = true;
        }

        public static bool? IsActive => _isActive.Value;

        public void Dispose()
        {
            _isActive.Value = default;
        }
    }
}
