using Lamar;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.Composition.Factory;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Composition.Factory.Definitions;

namespace RhDev.Common.Web.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public const string CONFIG_APP_ROOT = "Common";

        public static IServiceCollection AddCommonOptions(this IServiceCollection services)
        {
            services
                .AddOptions<CommonConfiguration>()
                .BindConfiguration(CONFIG_APP_ROOT);

            return services;
        }

        public static IServiceCollection ConfigureAppComposition(this IServiceCollection services, params ContainerRegistrationDefinition[] registrationDefinitions)
        {
            if (services is ServiceRegistry lamarServices) ApplicationContainerFactory.Create(lamarServices, registrationDefinitions);

            return services;
        }
    }
}
