using RhDev.Common.Web.Core.Composition.Factory.Definitions;
using RhDev.Common.Web.Core.Utils;
using System.Collections.Generic;

namespace RhDev.Common.Web.Core.Composition.Factory.Builder
{
    public class ContainerRegistrationDefinitionComponentBuilder : IContainerRegistrationDefinitionBuilder<ContainerRegistrationComponentDefinition>
    {
        private readonly string componentName;
        private IList<ContainerRegistrationLayerDefinition> layers;
        private ContainerRegistrationDefinitionComponentBuilder(string cn)
        {
            Guard.StringNotNullOrWhiteSpace(cn, nameof(cn));
            componentName = cn;
        }

        public ContainerRegistrationDefinitionComponentBuilder WithLayers(IList<ContainerRegistrationLayerDefinition> layers)
        {
            Guard.NotNull(layers, nameof(layers));

            this.layers = layers;

            return this;
        }

        public static ContainerRegistrationDefinitionComponentBuilder Get(string componentName) => new ContainerRegistrationDefinitionComponentBuilder(componentName);

        public ContainerRegistrationComponentDefinition Build()
        {
            return new ContainerRegistrationComponentDefinition
            {
                ComponentName = componentName,
                Layers = layers
            };
        }
    }
}
