using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Builder
{
    public interface IStateManagementBuilder<TPart> : IService where TPart : IWorkflowPartBuilder
    {
        TPart Build();
    }
}
