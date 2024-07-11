using System;
using System.Collections.Generic;
using System.Text;
using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Workflow.Monitor
{
    public interface IWorkflowRunnerPlanner : IAutoregisteredService
    {
        Task PlanAsync(int instanceEntityId);
    }
}
