using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow
{
    public enum WorkflowInstanceSystemProcessingInfoItemType
    {
        Unknown = 0,
        Info = 1,
        Fail = 1 << 1
    }
}
