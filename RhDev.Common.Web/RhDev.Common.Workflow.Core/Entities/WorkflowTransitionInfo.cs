using RhDev.Common.Workflow.Management;
using System;
using System.Collections.Generic;
using System.Text;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.Entities
{
    public class WorkflowTransitionInfo
    {
        public WorkflowInfo WorkflowInfo { get; set; }
        public IList<TransitionInfo> TransitionInfos { get; set; }
    }
}
