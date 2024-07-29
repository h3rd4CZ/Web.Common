using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhDev.Common.Workflow.Core.Security;
using System.Runtime.InteropServices;
using JasperFx.Core;
using RhDev.Customer.Component.Core.Impl.Data;

namespace RhDev.Customer.Component.Core.Impl.Workflow
{
    public class WorkflowGroupMembershipResolver : IWorkflowGroupMembershipResolver
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public WorkflowGroupMembershipResolver(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public async Task<ICollection<string>> GetAllGroupsAsync(string userIdentifier)
        {
            var user = await userManager.FindByIdAsync(userIdentifier);

            return await userManager.GetRolesAsync(user!);
        }

        public async Task<ICollection<string>> ResolveAsync(string groupIdentifier)
        {
            var role = await roleManager.FindByNameAsync(groupIdentifier);

            Guard.NotNull(role);

            return (await userManager.GetUsersInRoleAsync(role.Name!)).Select(u => u.Id).ToList();
        }
    }
}
