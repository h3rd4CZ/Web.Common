using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Core.Security;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;

namespace RhDev.Common.Workflow.Impl.Security
{
    public class UserInfoValueEvaluator : IUserInfoValueEvaluator
    {
        private readonly IDynamicDataStoreRepository<DbContext> dynamicDataStore;
        private readonly IWorkflowGroupMembershipResolver workflowGroupMembershipResolver;

        public UserInfoValueEvaluator(
            IDynamicDataStoreRepository<DbContext> dynamicDataStore,
            IWorkflowGroupMembershipResolver workflowGroupMembershipResolver)
        {
            this.dynamicDataStore = dynamicDataStore;
            this.workflowGroupMembershipResolver = workflowGroupMembershipResolver;
        }
        public async Task<StateManagementUserValue> EvaluateAsUserAsync(object user, SectionDesignation sectionDesignation)
        {
            IPrincipalInfo userInfo = default!;
                        
            if (Equals(null, user)) return default!;

            if (user is string)
            {
                string userAsStringData = (string)user;

                if(Guid.TryParse(userAsStringData, out _))
                {
                    var userPrincipal = await EvaluateAsUserAsync(userAsStringData, sectionDesignation);
                    return new StateManagementUserValue(userPrincipal, sectionDesignation);
                }
                else
                {
                    var gi = new PermissionGroupInfo(sectionDesignation, userAsStringData, userAsStringData, string.Empty, string.Empty);

                    return new StateManagementUserValue(gi, sectionDesignation);
                }
            }
            
            if (user is StateManagementUserValue muv)
            {
                return muv;

            }
            if (userInfo == default) return default!;

            return new StateManagementUserValue(userInfo, sectionDesignation);
        }

        public async Task<UserInfo> EvaluateAsUserAsync(string userId, SectionDesignation sectionDesignation)
        {
            Guard.StringNotNullOrWhiteSpace(userId);

            var userFound = await dynamicDataStore
                    .ReadEntityByLambdaAsync<IdentityUser>(typeof(IdentityUser), u => u.Id == userId);

            Guard.NotNull(userFound!);
                        
            return new UserInfo(sectionDesignation, userId, userFound!.UserName!, userFound.UserName!, userFound.Email!);
        }

        public async Task<IList<UserInfo>> EvaluateAsUsersAsync(string permissionGroupId, SectionDesignation sectionDesignation)
        {
            Guard.StringNotNullOrWhiteSpace(permissionGroupId);

            var groupUsers = await workflowGroupMembershipResolver.ResolveAsync(permissionGroupId);

            var userPrincipals = new List<UserInfo>();

            foreach (var groupUser in groupUsers)
            {
                var pi = await EvaluateAsUserAsync(groupUser, sectionDesignation);
                userPrincipals.Add(pi);
            }

            return userPrincipals;
        }
    }
}
