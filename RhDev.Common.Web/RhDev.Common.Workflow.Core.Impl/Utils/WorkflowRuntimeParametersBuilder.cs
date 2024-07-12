using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Impl.Utils
{
    public class WorkflowRuntimeParametersBuilder : IWorkflowRuntimeParametersBuilder
    {
        private StateMachineRuntimeParameters runtimeParameters;
        public StateMachineRuntimeParameters Build() => runtimeParameters;

        public WorkflowRuntimeParametersBuilder()
        {
            Init();
        }

        private void Init()
        {
            runtimeParameters = new StateMachineRuntimeParameters();

            runtimeParameters.UserData = StateManagementCommonTriggerProperties.Empty;

            runtimeParameters.TransitionType = TransitionRequestType.System;

            runtimeParameters.UserPermissionGroups = new List<string> { };
        }

        public IWorkflowRuntimeParametersBuilder Clear()
        {
            Init();

            return this;
        }

        public IWorkflowRuntimeParametersBuilder IsSystem(bool isSystem)
        {
            runtimeParameters.EvaluateSystemTriggers = isSystem;
            runtimeParameters.TransitionType = isSystem
                 ? TransitionRequestType.System
                 : TransitionRequestType.User;
            return this;
        }

        public IWorkflowRuntimeParametersBuilder MembershipLocally(bool membershipLocally)
        {
            runtimeParameters.EvaluateMembershipLocally = membershipLocally;
            return this;
        }
                
        public IWorkflowRuntimeParametersBuilder WithDocumentIdentifier(WorkflowDocumentIdentifier workflowDocumentIdentifier)
        {
            Guard.NotNull(workflowDocumentIdentifier, nameof(workflowDocumentIdentifier));

            runtimeParameters.DocumentMetadataIdentifier = workflowDocumentIdentifier;
            return this;
        }
                
        public IWorkflowRuntimeParametersBuilder WithUserData(StateManagementCommonTriggerProperties stateManagementCommonTriggerProperties)
        {
            Guard.NotNull(stateManagementCommonTriggerProperties, nameof(stateManagementCommonTriggerProperties));

            runtimeParameters.UserData= stateManagementCommonTriggerProperties;
            return this;
        }

        public IWorkflowRuntimeParametersBuilder WithUserGroups(List<string> userGroups)
        {
            Guard.NotNull(userGroups, nameof(userGroups));

            runtimeParameters.UserPermissionGroups = userGroups;
            return this;
        }

        public IWorkflowRuntimeParametersBuilder WithUserId(string userId)
        {

            runtimeParameters.UserId = userId;
            return this;
        }

        public IWorkflowRuntimeParametersBuilder WithWorkflowInfo(WorkflowInfo workflowInfo)
        {
            Guard.NotNull(workflowInfo, nameof(workflowInfo));

            runtimeParameters.Workflow = workflowInfo;
            return this;
        }
    }
}
