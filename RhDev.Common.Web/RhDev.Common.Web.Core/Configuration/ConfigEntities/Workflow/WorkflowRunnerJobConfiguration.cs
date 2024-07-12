using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Configuration.ConfigEntities.Workflow
{
    public class WorkflowRunnerJobConfiguration : IApplicationConfigurationSection
    {
        public string Path => $"{Parent!.Path}:WorkflowRunnerJob";

        public IApplicationConfigurationSection? Parent => WorkflowConfiguration.Get;

        public string Cron { get; set; } = "*/1 * * * *";
    }
}
