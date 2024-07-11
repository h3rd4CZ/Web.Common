using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Factory
{
    public class CommonDbContextFactory<TDBContext> : IDbContextFactory<TDBContext> where TDBContext : DbContext
    {
        private readonly IServiceProvider serviceProvider;

        public CommonDbContextFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public TDBContext CreateDbContext()
        {
            if (typeof(TDBContext) == typeof(DbContext))
            {
                var registeredContext = serviceProvider.GetService<DbContext>();

                if (registeredContext is null) throw new InvalidOperationException("DBContext using factory must be registered against real db context, no real dbcontext found");

                return (TDBContext)ActivatorUtilities.CreateInstance(serviceProvider, registeredContext.GetType());
            }
            else
            {
                return ActivatorUtilities.CreateInstance<TDBContext>(serviceProvider);
            }
        }
    }
}
