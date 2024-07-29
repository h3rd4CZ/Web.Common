using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RhDev.Common.Web.Core.Composition.Factory.Definitions;
using RhDev.Common.Web.Core.DataAccess.Sql.DependencyInjection;
using RhDev.Common.Web.Core.DependencyInjection;
using RhDev.Common.Web.Core.Impl.Timer;

namespace RhDev.Common.Web.Core.Impl.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddCommon<TDbContext>(
            this IServiceCollection services, HostBuilderContext hostBuilderContext,
            ContainerRegistrationDefinition[] containerRegistrationDefinitions = default!, string? connStringKey = default,
            bool useDbContextFactory = false, Action<CommonWebRegistrationBuilder>? b = default,
            bool useLazyLoadingProxies = true,
            params Type[] databaseSaveChangeInterceptorsTypes) 
            where TDbContext : DbContext
        {
            var builder = ActivateBuilder(b);
                        
            services.ConfigureDatabase<TDbContext>(hostBuilderContext.Configuration, useDbContextFactory, connStringKey, useLazyLoadingProxies,databaseSaveChangeInterceptorsTypes);

            services.AddCommonOptions();
             
            if(hostBuilderContext.HostingEnvironment.IsDevelopment()) services.AddDatabaseDeveloperPageExceptionFilter();

            if (builder.QueueHostedService)
            {
                services.AddQueueHostedService(hostBuilderContext.Configuration, builder);
            }

            services.ConfigureAppComposition(containerRegistrationDefinitions);

            return services;
        }

        public static CommonWebRegistrationBuilder ActivateBuilder(Action<CommonWebRegistrationBuilder>? b)
        {
            if (b is null) return new();

            var builder = new CommonWebRegistrationBuilder();

            b(builder);

            return builder;
        }
    }
}
