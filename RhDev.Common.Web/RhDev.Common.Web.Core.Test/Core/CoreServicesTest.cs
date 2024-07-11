using Castle.Core.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog.LayoutRenderers;
using NSubstitute.Exceptions;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Impl;
using RhDev.Customer.Component.Core.Impl;
using RhDev.Customer.Component.Core.Impl.Options;
using Xunit.Abstractions;

namespace RhDev.Common.Web.Core.Test.Core
{
    public class CoreServicesTest : IntegrationTestBase
    {
        private const string ENV_PREFIX = "TEST_";

        public CoreServicesTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact]
        public void Host_Built_Composition_TestBase()
        {
            var host = Host();

            var services = host.Services;

            var clockProvider = services.GetRequiredService<ICentralClockProvider>();

            clockProvider.Should().NotBeNull().And.BeOfType<CentralClockProvider>();
        }

        [Fact]
        public void Host_Built_ClientComposition_TestBase()
        {
            var host = Host(new[] { TestCompositionDefinition.GetDefinition() });

            var services = host.Services;

            var foo = services.GetRequiredService<IFoo>();

            foo.Should().NotBeNull().And.BeOfType<Foo>();

            foo.Boo();
        }

        [Fact]
        public void ReadingConfiguration_WithEnvVars_BaseTest()
        {
            int expected = 50;

            Environment.SetEnvironmentVariable($"{ENV_PREFIX}Common__QueueService__QueueCapacity", $"{expected}", EnvironmentVariableTarget.Process);

            var host = Host(new[] {ENV_PREFIX }, new[] { TestCompositionDefinition.GetDefinition() });

            var services = host.Services;

            var conf = services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

            var common = conf.GetSection("Common").Get<CommonConfiguration>();

            var commonOptions = host.Services.GetRequiredService<IOptionsSnapshot<CommonConfiguration>>();

            commonOptions.Value.Should().NotBeNull();

            var val = commonOptions.Value;

            val.QueueService.QueueCapacity.Should().Be(expected);
            val.Identity.CookieExpirationInMinutes.Should().Be(100);
            val.Identity.SignoutInactivityIntervalInMinutes.Should().Be(100);
        }

        [Fact]
        public void Registered_CustomOptions_Works()
        {
            var host = Host(new[] { TestCompositionDefinition.GetDefinition() });
            var services = host.Services;

            var customerOptions = services.GetRequiredService<IOptions<CustomerOptions>>();

            customerOptions.Value.Should().NotBeNull();

            customerOptions.Value.Path.Should().NotBeEmpty();
            customerOptions.Value.Name.Should().NotBeEmpty();
            customerOptions.Value.Size.Should().BeGreaterThan(0);
        }
    }
}