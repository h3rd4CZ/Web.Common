using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Monitor
{
    public interface IWatchDog : IAutoregisteredService
    {
        void Run();
    }
}
