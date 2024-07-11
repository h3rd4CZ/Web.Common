using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow
{
    public interface IStateManagementCleanUpRunner : IAutoregisteredService
    {
        Task RunAsync();
    }
}
