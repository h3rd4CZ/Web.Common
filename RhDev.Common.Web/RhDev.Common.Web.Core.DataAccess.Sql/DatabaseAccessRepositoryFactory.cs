using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.Utils;
using System.Threading;

namespace RhDev.Common.Web.Core.DataAccess.Sql
{
    public class DatabaseAccessRepositoryFactory<TDatabase> : IDatabaseAccessRepositoryFactory<TDatabase> where TDatabase : DbContext
    {
        private readonly IServiceProvider serviceProvider;      
                                
        public DatabaseAccessRepositoryFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
                
        public async Task RunActionAsync(Func<TDatabase, Task> action)
        {
            Guard.NotNull(action, nameof(action));

            var factory = serviceProvider.GetService<IDbContextFactory<TDatabase>>();

            if (factory is not null)
            {
                using (var db = factory.CreateDbContext()) await action(db);
            }
            else
            {
                var database = (TDatabase)serviceProvider.GetService(typeof(TDatabase));

                Guard.NotNull(database, nameof(database));

                await action(database);
            }
        }
    }
}
