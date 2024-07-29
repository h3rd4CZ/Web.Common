using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;
using RhDev.Common.Web.Core.Utils;
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
        private readonly WorkflowDatabaseTestContext db;
        private readonly UserManager<IdentityUser> userManager;

        public TestWorkflowGroupMembershipResolver(
            WorkflowDatabaseTestContext db,
            UserManager<IdentityUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        public async Task<ICollection<string>> GetAllGroupsAsync(string userIdentifier)
        {
            var user = await userManager.FindByIdAsync(userIdentifier);

            return await userManager.GetRolesAsync(user!);
        }
        
        public async Task<ICollection<string>> ResolveAsync(string groupIdentifier)
        {
            var roleStore = new RoleStore<IdentityRole>(db);
                        
            var role = await roleStore.Roles.FirstOrDefaultAsync(r => r.Name == groupIdentifier);

            Guard.NotNull(role);

            var usersInRole = await db.UserRoles.Where(ur => ur.RoleId == role.Id).ToListAsync();

            return usersInRole.Select(ur => ur.UserId).ToList();
        }
    }
}
