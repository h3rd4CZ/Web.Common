using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Composition;
using System;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.DataAccess.SQL
{
    public interface IDatabaseAccessRepositoryFactory<TDatabase> : IService where TDatabase : DbContext
    {
        Task RunActionAsync(Func<TDatabase, Task> action);
    }
}
