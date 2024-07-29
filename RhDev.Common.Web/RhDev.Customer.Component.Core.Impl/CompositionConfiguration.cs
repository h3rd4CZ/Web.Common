using FluentAssertions.Common;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Workflow.Core.Security;
using RhDev.Customer.Component.Core.Impl.Options;
using RhDev.Customer.Component.Core.Impl.Workflow;
using Container = Lamar.Container;

namespace RhDev.Customer.Component.Core.Impl
{
    public class CompositionConfiguration : ConventionConfigurationBase
    {
        public CompositionConfiguration(ServiceRegistry configuration, Container container) : base(configuration, container)
        {
        }

        public override void Apply()
        {
            base.Apply();

            Configuration
                .AddOptions<CustomerOptions>()
                .BindConfiguration(new CustomerOptions().Path);

            For<IWorkflowGroupMembershipResolver>().Use<WorkflowGroupMembershipResolver>();
        }
    }
}
