using RhDev.Common.Web.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Entities
{
    public class TaskCompletitionInfo
    {
        public string AssignedTo { get; set; }
        public string? ResolvedBy { get; set; }
        public string GroupId { get; set; }
        public CentralClock AssignedOn { get; set; }
        public CentralClock ResolvedOn { get; set; }
        public List<KeyValuePair<string, string>> UserParameters { get; set; }
        public string SelectedTrigger { get; set; }
        public bool NotFound { get; set; }
    }
}
