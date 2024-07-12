using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Configuration.ConfigEntities.Workflow
{
    public class WorkflowCleanupJobConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent!.Path}:WorkflowCleanupJob";

        public IApplicationConfigurationSection? Parent => WorkflowConfiguration.Get;

        public string Cron { get; set; } = "0 1 * * *";
    }
}
