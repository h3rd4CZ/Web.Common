using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    public interface IPrincipalInfo
    {
        string Id { get; }
        string Name { get; }
        string DisplayName { get; }
        public bool IsPermissionGroup { get; }
        bool IsValid { get; }
        SectionDesignation SectionDesignation { get; }
    }
}
