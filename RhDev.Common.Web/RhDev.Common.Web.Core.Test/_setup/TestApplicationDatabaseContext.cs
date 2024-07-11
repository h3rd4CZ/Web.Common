using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.Sql;

namespace RhDev.Common.Web.Core.Test._setup
{
    public class TestApplicationDatabaseContext : CommonDatabaseContext
    {
        public TestApplicationDatabaseContext(DbContextOptions<TestApplicationDatabaseContext> dbContextOptions) : base(dbContextOptions) { }
    }
}
