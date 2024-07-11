using RhDev.Common.Web.Core.Composition.Factory.Builder;
using RhDev.Common.Web.Core.Composition.Factory.Definitions;

namespace RhDev.Customer.Component.Core.Impl
{
    public class TestCompositionDefinition
    {
        public static ContainerRegistrationDefinition GetDefinition() => ContainerRegistrationDefinitionBuilder
                .Get(Constants.SOLUTION_PREFIX)
                .WithComponents(new List<ContainerRegistrationComponentDefinition>
                {
                    ContainerRegistrationDefinitionComponentBuilder.Get(Common.Web.Core.Constants.COMPONENT_CORE_NAME)
                        .WithLayers(
                            new List<ContainerRegistrationLayerDefinition>{
                                ContainerRegistrationDefinitionLayerBuilder.GetNativeImplementationLayer().Build()
                        }).Build()
                }).Build();
    }
}
