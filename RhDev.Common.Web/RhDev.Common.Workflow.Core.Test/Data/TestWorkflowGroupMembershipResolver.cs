using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RhDev.Common.Workflow.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Test.Data
{
    public class TestWorkflowGroupMembershipResolver : IWorkflowGroupMembershipResolver
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public TestWorkflowGroupMembershipResolver(
            UserManager<IdentityUser> userManager,  
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<ICollection<string>> GetAllGroupsAsync(string userIdentifier)
        {
            var user = await userManager.FindByIdAsync(userIdentifier);

            return await userManager.GetRolesAsync(user!);
        }

        public async Task<ICollection<string>> ResolveAsync(string groupIdentifier)
        {
            var role = await roleManager.FindByIdAsync(groupIdentifier);

            var roles = await userManager.GetUsersInRoleAsync(role.Name);

            return roles.Select(r => r.Id).ToList();
        }
    }
}
