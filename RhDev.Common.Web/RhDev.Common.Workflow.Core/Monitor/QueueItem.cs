using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Monitor
{
    public class QueueItem
    {
        public bool Reserved { get; set; }
        public WorkflowInstance Value { get; }

        public QueueItem(WorkflowInstance workflowInstance)
        {
            Value = workflowInstance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(QueueItem)) return false;

            return Value.Id == ((QueueItem)obj).Value.Id;
        }

        public override int GetHashCode()
        {
            return Value.Id.GetHashCode();
        }
    }
}
