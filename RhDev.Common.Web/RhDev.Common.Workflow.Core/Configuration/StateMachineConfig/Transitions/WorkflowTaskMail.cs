using System.Xml.Serialization;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    public class WorkflowTaskMail
    {
        public WorkflowTaskMailText Text { get; set; } = new WorkflowTaskMailText();
        public WorkflowTaskMailSubject Subject { get; set; } = new WorkflowTaskMailSubject();
    }
}
