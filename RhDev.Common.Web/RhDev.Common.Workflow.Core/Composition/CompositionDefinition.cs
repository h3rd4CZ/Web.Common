using RhDev.Common.Web.Core.Composition.Factory.Builder;
using RhDev.Common.Web.Core.Composition.Factory.Definitions;
using RhDev.Common.Workflow.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Composition
{
    public class CompositionDefinition
    {
        public static ContainerRegistrationDefinition GetDefinition() => ContainerRegistrationDefinitionBuilder
                .Get(GlobalVariable.SOLUTION_PREF)
                .WithComponents(new List<ContainerRegistrationComponentDefinition>
                {
                    ContainerRegistrationDefinitionComponentBuilder.Get(GlobalVariable.COMPONENT_CORE_NAME)
                        .WithLayers(
                            new List<ContainerRegistrationLayerDefinition>{
                                ContainerRegistrationDefinitionLayerBuilder.Get("DataAccess.Sql").Build(),
                                ContainerRegistrationDefinitionLayerBuilder.GetNativeImplementationLayer().Build()
                        }).Build()
                }).Build();
    }
}
