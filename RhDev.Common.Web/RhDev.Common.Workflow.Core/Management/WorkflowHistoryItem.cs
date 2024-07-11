using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Management
{
    public class WorkflowHistoryItem
    {
        public DateTime Date { get; set; }
        public string User { get; set; }
        public string Event { get; set; }
        public string Message { get; set; }
    }
}
