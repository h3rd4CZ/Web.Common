using Lamar;
using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Workflow.Core.DataAccess.Sql
{
    public class CompositionConfiguration : ConventionConfigurationBase
    {
        public CompositionConfiguration(ServiceRegistry configuration, Container container) : base(configuration, container) { }
        
        public override void Apply()
        {
            base.Apply();
        }
    }
}
