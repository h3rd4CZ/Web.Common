using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using System;
using System.Collections.Generic;
using System.Text;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.Management
{
    public class WorkflowDocumentItemProperty
    {
        public string FieldInternalName { get; set; }
        public WorkflowDataType WorkflowDataType { get; set; }
        public string Value { get; set; }
    }
}
