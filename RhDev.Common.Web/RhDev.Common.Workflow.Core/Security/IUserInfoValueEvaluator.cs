using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.Security
{
    public interface IUserInfoValueEvaluator : IAutoregisteredService
    {
        Task<StateManagementUserValue> EvaluateAsUserAsync(object value, SectionDesignation sectionDesignation);
        Task<UserInfo> EvaluateAsUserAsync(string userId, SectionDesignation sectionDesignation);
        Task<IList<UserInfo>> EvaluateAsUsersAsync(string permissionGroupId, SectionDesignation sectionDesignation);
    }
}
