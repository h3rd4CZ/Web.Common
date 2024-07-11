using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Composition;
using System;

namespace RhDev.Common.Web.Core.DataAccess.SQL
{
    public interface IDatabaseAccessRepository<TDatabase> : IService, IDisposable where TDatabase : DbContext
    {
        TDatabase Database { get; }
    }
}
