using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Security
{
    public interface IWorkflowGroupMembershipResolver : IService
    {
        /// <summary>
        /// Returns users in thid group as Collection of identifiers
        /// </summary>
        /// <param name="groupIdentifier"></param>
        /// <returns></returns>
        Task<ICollection<string>> ResolveAsync(string groupIdentifier);

        
        /// <summary>
        /// Returns all groups the user is member
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <returns></returns>
        Task<ICollection<string>> GetAllGroupsAsync(string userIdentifier);
    }
}
