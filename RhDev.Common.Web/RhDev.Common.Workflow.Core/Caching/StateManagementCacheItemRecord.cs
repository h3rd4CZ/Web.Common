namespace RhDev.Common.Workflow.Caching
{
    public enum StateManagementCacheType
    {
        Unknown = 0,
        ConfigurationFile = 1,
        GroupIdentificator = 1 << 1
    }

    public class StateManagementCacheItemRecord
    {
        public string Server { get; set; }
        public StateManagementCacheType CacheType { get; set; }
        public string Key { get; set; }
        public string DataType { get; set; }
    }
}
