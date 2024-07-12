using JasperFx.CodeGeneration.Frames;
using JasperFx.Core;
using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow;
using RhDev.Common.Workflow.Core.Security;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.DataAccess.SharePoint.Online.Security
{
    public class WorkflowMembershipProvider : IWorkflowMembershipProvider
    {
        private readonly IUserInfoValueEvaluator userInfoValueEvaluator;
        private readonly IWorkflowGroupMembershipResolver workflowGroupMembershipResolver;
                        
        public WorkflowMembershipProvider(
            IUserInfoValueEvaluator userInfoValueEvaluator,
            IWorkflowGroupMembershipResolver workflowGroupMembershipResolver
            )
        {
            this.userInfoValueEvaluator = userInfoValueEvaluator;
            this.workflowGroupMembershipResolver = workflowGroupMembershipResolver;
        }
               
        public async Task<IList<UserInfo>> ExtractGroupMembersAsync(StateManagementUserValue userValue, SectionDesignation sectionDesignation)
        {
            Guard.NotNull(userValue, nameof(userValue));
            Guard.NotNull(sectionDesignation, nameof(sectionDesignation));

            if (userValue.IsPermissionGroup)
            {
                return await userInfoValueEvaluator.EvaluateAsUsersAsync(userValue.Id, sectionDesignation);
            }
            else throw new InvalidOperationException("User is not a permission group");
        }

        public async Task<IList<string>> GetAllUserPermissionGroupsAsync(SectionDesignation sectionDesignation, string userId)
        {
            Guard.StringNotNullOrWhiteSpace(userId);
            Guard.NotNull(sectionDesignation);

            var userPermissions = await workflowGroupMembershipResolver.GetAllGroupsAsync(userId);

            return 
                userPermissions.Distinct().ToList();
        }
    }
}
