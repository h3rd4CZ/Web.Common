using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Test.Data
{
    public class WorkflowDatabaseTestContext : CommonIdentityDatabaseContext
    {
        public WorkflowDatabaseTestContext(DbContextOptions<WorkflowDatabaseTestContext> options)
            : base(options)
        {
        }
    }
}
