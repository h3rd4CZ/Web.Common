using RhDev.Common.Workflow.Builder;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    [Serializable]
    public class WorkflowAssigneeTask : IWorkflowPartBuilder
    {
        public WorkflowTaskRespondType TaskRespondeType { get; set; }

        public List<Operand> Assignee { get; set; } = new List<Operand>();

        public bool GroupExtract { get; set; }

        public bool CopyRoleAssignment { get; set; }

        public bool KeepExistingAssignment { get; set; }

        public bool CurrentReadOnly { get; set; }

        public List<PermissionSet> Permission { get; set; } = new List<PermissionSet>();

        public WorkflowTaskMail Mail { get; set; } = new WorkflowTaskMail();
    }
}
