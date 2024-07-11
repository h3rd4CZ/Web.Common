using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.DataAccess.Workflow
{
    public record class WorkflowTransitionRequestRollbackInfo(DateTime rollbackedAt, bool successfully, string message);
}
