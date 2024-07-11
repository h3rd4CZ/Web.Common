using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Management;
using System;
using System.Collections.Generic;
using System.Text;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow.Utils
{
    public interface IWorkflowRuntimeParametersBuilder : IAutoregisteredService
    {
        StateMachineRuntimeParameters Build();

        IWorkflowRuntimeParametersBuilder Clear();
        IWorkflowRuntimeParametersBuilder WithUserData(StateManagementCommonTriggerProperties stateManagementCommonTriggerProperties);
        IWorkflowRuntimeParametersBuilder WithDocumentIdentifier(WorkflowDocumentIdentifier workflowDocumentIdentifier);
        IWorkflowRuntimeParametersBuilder WithWorkflowInfo(WorkflowInfo workflowInfo);
        IWorkflowRuntimeParametersBuilder WithUserId(string userId);
        IWorkflowRuntimeParametersBuilder WithUserGroups(List<string> userGroups);
        IWorkflowRuntimeParametersBuilder IsSystem(bool isSystem);
        IWorkflowRuntimeParametersBuilder MembershipLocally(bool membershipLocally);
    }
}
