using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Configuration.ConfigEntities.Workflow
{
    public class WorkflowConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent!.Path}:Workflow";

        public IApplicationConfigurationSection? Parent => CommonConfiguration.Get;

        public WorkflowRunnerJobConfiguration WorkflowRunnerJob { get; set; } = new();
        public WorkflowCleanupJobConfiguration WorkflowCleanupJob { get; set; } = new();
        public WorkflowFailedRequestsRunnerJobConfiguration WorkflowFailedRequestsRunnerJob { get; set; } = new();
        public int DistributedLockTimeoutInSeconds { get; set; } = 30;
        public int DeleteFinishedInstancesAfterDays { get; set; } = 9999;
        public int DeleteRequestQueueItemsInDays { get; set; } = 14;
        public int NumOfRepeatOfFailRequest { get; set; } = 4;

        public static WorkflowConfiguration Get => new WorkflowConfiguration();
    }
}
