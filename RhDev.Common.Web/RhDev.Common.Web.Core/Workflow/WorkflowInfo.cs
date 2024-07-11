using System;

namespace RhDev.Common.Web.Core.Workflow
{
    public class WorkflowInfo
    {
        public string Name { get; set; }
        public string InstanceId { get; set; }
        public long DataId { get; set; }
        public string Version { get; set; }
        public DateTime Started { get; set; }
        public string? Initiator { get; set; }

        public static WorkflowInfo ForDataId(long dataId) => new WorkflowInfo
        {
            DataId = dataId
        };
    }
}
