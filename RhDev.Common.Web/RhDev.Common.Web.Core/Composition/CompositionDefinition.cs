using RhDev.Common.Web.Core.Composition.Factory.Builder;
using RhDev.Common.Web.Core.Composition.Factory.Definitions;

namespace RhDev.Common.Web.Core.Composition
{
    public static class CompositionDefinition
    {
        public static ContainerRegistrationDefinition GetDefinition() => ContainerRegistrationDefinitionBuilder
                .Get(Constants.SOLUTION_PREFIX)
                .WithComponents(new List<ContainerRegistrationComponentDefinition>
                {
                    ContainerRegistrationDefinitionComponentBuilder.Get(Constants.COMPONENT_CORE_NAME)
                        .WithLayers(
                            new List<ContainerRegistrationLayerDefinition>{
                                ContainerRegistrationDefinitionLayerBuilder.Get(Constants.LAYER_SQL_NAME).Build(),
                                ContainerRegistrationDefinitionLayerBuilder.GetNativeImplementationLayer().Build()
                        }).Build()
                }).Build();
    }
}
