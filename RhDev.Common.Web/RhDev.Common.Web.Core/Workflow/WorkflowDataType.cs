using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    public enum WorkflowDataType
    {
        Unknown = 0,
        Text = 1,
        Number = 1 << 2,
        Boolean = 1 << 3,
        DateTime = 1 << 4,
        User = 1 << 5,
        Array = 1 << 6,
        Null = 1 << 7,
        Url = 1 << 10
    }
}
