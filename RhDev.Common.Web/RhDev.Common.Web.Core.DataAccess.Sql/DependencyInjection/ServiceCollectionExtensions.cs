using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.DataAccess.Sql.Factory;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.DataAccess.Sql.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection ConfigureDatabase<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool addDbContextFactory,
            string? connStringKey = default,
            bool useLazyLoadingProxies = true,
            params Type[] databaseSaveChangeInterceptorsTypes) where TDbContext : DbContext
        {
            if (typeof(TDbContext) == typeof(DbContext)) return services;

            var isCommonDbContext = CheckForCommonDatabaseRuntimeType<TDbContext>();

            if (databaseSaveChangeInterceptorsTypes is not null and { Length: > 0 })
            {
                foreach (var interceptorType in databaseSaveChangeInterceptorsTypes)
                {
                    services.AddScoped(typeof(ISaveChangesInterceptor), interceptorType);
                }
            }
                        
            services.AddDbContext<TDbContext>((s, o) =>
            {
                if (useLazyLoadingProxies) o.UseLazyLoadingProxies();

                if (databaseSaveChangeInterceptorsTypes is not null and { Length: > 0 })
                {
                    o.AddInterceptors(s.GetServices<ISaveChangesInterceptor>()!);
                }

                var connString = string.IsNullOrWhiteSpace(connStringKey)
                    ? configuration.GetConnectionString(Constants.DEFAULTCONNECTION_KEY)
                    : configuration.GetValue<string>(connStringKey);

                Guard.StringNotNullOrWhiteSpace(connString);

                o.UseSqlServer(connString);
            });

            if (addDbContextFactory) services.AddScoped<IDbContextFactory<TDbContext>, CommonDbContextFactory<TDbContext>>();

            if (isCommonDbContext)
            {
                services.AddScoped<DbContext, TDbContext>();

                if (addDbContextFactory) services.AddScoped<IDbContextFactory<DbContext>, CommonDbContextFactory<DbContext>>();
            }

            return services;
        }

        public static bool CheckForCommonDatabaseRuntimeType(Type dbContextType) =>
               typeof(CommonDatabaseContext).IsAssignableFrom(dbContextType) ||
               IsSubclassOfRawGeneric(typeof(CommonIdentityDatabaseContext<>), dbContextType) ||
               IsSubclassOfRawGeneric(typeof(CommonIdentityDatabaseContext<,,>), dbContextType) ||
               IsSubclassOfRawGeneric(typeof(CommonIdentityDatabaseContext<,,,,,,,>), dbContextType);

        public static bool CheckForCommonDatabaseRuntimeType<TDbContext>() where TDbContext : DbContext => CheckForCommonDatabaseRuntimeType(typeof(TDbContext));

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
