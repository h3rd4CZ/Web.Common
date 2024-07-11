using RhDev.Common.Web.Core.Utils;
using System;
using System.Collections.Generic;

namespace RhDev.Common.Web.Core.Composition.Factory.Definitions
{
    public class ContainerRegistrationDefinition : IContainerRegistrationDefinition
    {
        public const string DEFAULT_COMPOSITION_CONFIGURATION_TYPE_NAME = "CompositionConfiguration";

        public const string ASSEMBLY_FULL_NAME_FORMAT = "{0}";

        private readonly string solutionNamespacePrefix;
        private readonly IList<ContainerRegistrationComponentDefinition> componentDefinitions;

        public static ContainerRegistrationDefinition Empty => new ContainerRegistrationDefinition("Empty", new List<ContainerRegistrationComponentDefinition> { });

        public ContainerRegistrationDefinition(string solutionNamespacePrefix, IList<ContainerRegistrationComponentDefinition> componentDefinitions)
        {
            Guard.StringNotNullOrWhiteSpace(solutionNamespacePrefix, nameof(solutionNamespacePrefix));
            Guard.NotNull(componentDefinitions, nameof(componentDefinitions));

            this.solutionNamespacePrefix = solutionNamespacePrefix;
            this.componentDefinitions = componentDefinitions;
        }

        public IEnumerable<Type> BuildTypes()
        {
            foreach (var component in componentDefinitions)
            {
                if (!component.Isvalid()) continue;

                foreach (var layer in component.Layers)
                {
                    if (!layer.IsValid()) continue;

                    var assemblyName = $"{solutionNamespacePrefix}.{component.ComponentName}.{layer.LayerName}";

                    Type frontendType =
                        Type.GetType(string.Format(ASSEMBLY_FULL_NAME_FORMAT, $"{assemblyName}.{DEFAULT_COMPOSITION_CONFIGURATION_TYPE_NAME}, {assemblyName}"), false);

                    ValidateBuildType(frontendType, component, layer);

                    yield return frontendType;

                }
            }
        }

        private void ValidateBuildType(Type type, ContainerRegistrationComponentDefinition componentDefinition, ContainerRegistrationLayerDefinition layerDefinition)
        {
            if (!Equals(null, type)) return;

            Guard.NotNull(componentDefinition, nameof(componentDefinition));
            Guard.NotNull(layerDefinition, nameof(layerDefinition));

            throw new InvalidOperationException($"Type definition resolving failed for component : {componentDefinition}, layer : {layerDefinition}");
        }
    }
}
