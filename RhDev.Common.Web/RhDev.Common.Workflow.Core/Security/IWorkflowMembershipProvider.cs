using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.Security
{
    public interface IWorkflowMembershipProvider : IAutoregisteredService
    {
        Task<IList<UserInfo>> ExtractGroupMembersAsync(StateManagementUserValue userValue, SectionDesignation sectionDesignation);
        Task<IList<string>> GetAllUserPermissionGroupsAsync(SectionDesignation sectionDesignation, string userId);
    }
}
