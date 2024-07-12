using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Monitor
{
    public interface IWorkflowRunnerQueue : IAutoregisteredService
    {
        void Enqueue(WorkflowInstance value);
        QueueItem BeginDequeue(int millisecondsTimeout);
        QueueItem BeginDequeue();
        void CompleteDequeue(QueueItem item);
        void Clear();
    }
}
