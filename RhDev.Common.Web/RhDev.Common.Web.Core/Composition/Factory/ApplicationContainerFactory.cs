using Lamar;
using Lamar.IoC.Setters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RhDev.Common.Web.Core.Composition.Factory.Definitions;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Composition.Factory
{
    public static class ApplicationContainerFactory
    {
        private static volatile object _lock = new object();

        private static Container _container;

        public static IApplicationContainer? Root => _container is null ? default : new ApplicationContainer(_container);

        public static void Create(ServiceRegistry sr, params ContainerRegistrationDefinition[] containerDefinitions)
        {
            BuildContainerRegistrationDefinition(CompositionDefinition.GetDefinition(), sr);

            if (containerDefinitions is not null and { Length: > 0 })
            {
                foreach (var reg in containerDefinitions.Where(r => r is not null)) BuildContainerRegistrationDefinition(reg!, sr);
            }
        }

        private static void BuildContainerRegistrationDefinition(ContainerRegistrationDefinition definition, ServiceRegistry sr)
        {
            var solutionTypesDefinitionBuild = definition.BuildTypes()?.ToList();

            BuildContainerForTypes(solutionTypesDefinitionBuild!, sr);
        }

        private static void BuildContainerForTypes(IList<Type> types, ServiceRegistry sr)
        {
            ApplyConfiguration(sr, types);
        }

        private static void MatchPropertyConvention(SetterConvention convention)
        {
            convention.TypeMatches(t => typeof(IService).IsAssignableFrom(t) ||
                                        typeof(IService[]).IsAssignableFrom(t));
        }



        private static void ApplyConfiguration(ServiceRegistry configuration, IEnumerable<Type> configurationTypes)
        {
            var container = new Container(s => { });

            foreach (Type configurationType in configurationTypes)
            {
                var compositionConfiguration =
                    (CompositionConfigurationBase)Activator.CreateInstance(configurationType, configuration, container);

                compositionConfiguration.Apply();
            }

            container.Configure(configuration);

            lock (_lock) _container = container;

            configuration.Policies.SetAllProperties(MatchPropertyConvention);
        }

        /// <summary>
        /// !!!Use only for unit testing, inner container is intended for runtime injecting, mocking objects must be injected through this method
        /// </summary>
        /// <param name="sd"></param>
        public static void UpdateUnderlayingContainerConfiguration(ServiceDescriptor sd)
        {
            Guard.NotNull(_container, nameof(_container), "Cotainer is null, before apply configuration container must be created");
            Guard.NotNull(sd, nameof(sd));

            _container.Configure(s =>
            {
                s.Replace(sd);
            });
        }
    }
}
