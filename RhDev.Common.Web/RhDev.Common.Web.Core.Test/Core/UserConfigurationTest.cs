using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;
using RhDev.Common.Web.Core.Extensions;
using RhDev.Common.Web.Core.Security;
using RhDev.Common.Web.Core.Test._setup;
using RhDev.Customer.Component.Core.Impl.Data;
using System.Globalization;
using Xunit.Abstractions;

namespace RhDev.Common.Web.Core.Test.Core
{
    public class UserConfigurationTest : IntegrationTestBase
    {
        private const string BASEURI = "baseuri";

        public UserConfigurationTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            RegisterEfInMemoryMock<TestApplicationDatabaseContext, DbContext>(ctx =>
            {
                ctx.AddRange(new List<ApplicationUserSettings>
                {
                    new ApplicationUserSettings(){ Id = 1, Key = "Common:BaseUri", Value = BASEURI, Changed = DateTimeOffset.Now, ChangedBy = "SYSTEM"}
                });

                ctx.SaveChanges();
            });
        }

        [Fact]
        public void TestSqlConfigurationProviderWorksAndRealoadDataFromDatabase()
        {
            var host = Host(useSqlServerConfigurationProvider: true);

            var services = host.Services;

            var configurationRoot = services.GetRequiredService<IConfiguration>();

            ((IConfigurationRoot)configurationRoot).Reload();

            var options = services.GetRequiredService<IOptionsSnapshot<CommonConfiguration>>();

            options.Value.BaseUri.Should().Be(BASEURI);
        }

        [Fact]
        public async Task ReadingOptionsInBulk_Test()
        {
            var host = Host(useSqlServerConfigurationProvider: true);

            var services = host.Services;

            var ucp = services.GetRequiredService<IUserConfigurationProvider>();
            var lcp = services.GetRequiredService<ILiveConfigurationProvider>();

            var commonOptions = lcp.GetConfiguration<CommonConfiguration>();

            commonOptions.BaseUri.Should().Be(BASEURI);

            await ApplicationConfigurationWalker.WalkConfiguration(commonOptions, async (pi, section, key) =>
            {
                testOutputHelper!.WriteLine($"Prop KEY : {key}, VALUE : {pi.GetValue(section)}");
            });
        }

        [Fact]
        public async Task WritingOptionsInBulk_Test()
        {
            var host = Host();

            var services = host.Services;

            var database = services.GetRequiredService<DbContext>() as TestApplicationDatabaseContext;

            var ucp = services.GetRequiredService<IUserConfigurationProvider>();
            var lcp = services.GetRequiredService<ILiveConfigurationProvider>();

            var commonOptions = lcp.GetConfiguration<CommonConfiguration>();

            commonOptions.OptionsReloadInSeconds = 999;
            commonOptions.SmtpServer.Sender = "SUPERSENDER";
            commonOptions.SmsGate.PhoneCountryCodes = new List<string> { "420", "430" };

            await ucp.WriteConfigurationAsync(commonOptions, "SYSTEM");

            var dbSettings = database!.ApplicationUserSettings.ToList();

            dbSettings.Should().NotBeEmpty();

            dbSettings.FirstOrDefault(d => d.Key == "Common:SmtpServer:Sender").Value.Should().NotBeNull().And.Be("SUPERSENDER");
            dbSettings.FirstOrDefault(d => d.Key == "Common:SmsGate:PhoneCountryCodes:0").Value.Should().NotBeNull().And.Be("420");
            dbSettings.FirstOrDefault(d => d.Key == "Common:SmsGate:PhoneCountryCodes:1").Value.Should().NotBeNull().And.Be("430");
        }

        [Fact]
        public async Task FloatingPoints_Test()
        {
            var host = Host(useSqlServerConfigurationProvider: true);

            var services = host.Services;

            var ucp = services.GetRequiredService<IUserConfigurationProvider>();
            var lcp = services.GetRequiredService<ILiveConfigurationProvider>();

            var testConf = lcp.GetConfiguration<TestConfiguration>();

            testConf.Diff = 0.5f;

            //CultureInfo culture = new CultureInfo("cs-CZ"); // Change to the desired culture
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;

            await ucp.WriteConfigurationPropertyAsync(testConf, c => c.Diff, "SYSTEM");

            var conf = lcp.GetConfiguration<TestConfiguration>();
        }

        [Fact]
        public async Task NonPrimitveTypeAsArrayElement_Test()
        {
            var host = Host(useSqlServerConfigurationProvider: true);

            var services = host.Services;

            var ucp = services.GetRequiredService<IUserConfigurationProvider>();
            var lcp = services.GetRequiredService<ILiveConfigurationProvider>();

            var commonOptions = lcp.GetConfiguration<TestConfiguration>();

            commonOptions.Delay = 111;
            commonOptions.Settings.Age = 1;
            commonOptions.Addresses = new() {
                new ConfigurationAddress { City = "Prague", Zip = "111" },
                new ConfigurationAddress { City = "Brno", Zip = "112",
                    Metadata = new(){ new ConfigurationAddressMetadata
                {
                     Description = new ConfigurationAddressMetadataDescription{ Created = "Now" },
                     Population = 10000,
                     QoL = 5
                } } }
            };

            await ucp.WriteConfigurationAsync(commonOptions, "SYSTEM");

            var database = services.GetRequiredService<DbContext>() as TestApplicationDatabaseContext;

            var dbSettings = database!.ApplicationUserSettings.ToList();

            dbSettings.Should().NotBeEmpty();

            dbSettings.FirstOrDefault(d => d.Key == "Test:Delay")?.Value.Should().NotBeNull().And.Be("111");

            foreach (var settings in dbSettings)
            {
                testOutputHelper.WriteLine($"Key : {settings.Key}, value : {settings.Value}");
            }

            var commonOptionsRefresh = lcp.GetConfiguration<TestConfiguration>();

            commonOptionsRefresh.Addresses[1].Metadata[0].Population.Should().Be(10000);
        }

        [Fact]
        public async Task WritingOptionsSeparately_Test()
        {
            var host = Host(useSqlServerConfigurationProvider: true);

            var services = host.Services;

            var ucp = services.GetRequiredService<IUserConfigurationProvider>();
            var lcp = services.GetRequiredService<ILiveConfigurationProvider>();

            var commonOptions = lcp.GetConfiguration<CommonConfiguration>();

            //commonOptions.OptionsReloadInSeconds = 888;
            commonOptions.SmsGate.PhoneCountryCodes = new List<string> { "1", "2" };

            //await ucp.WriteConfigurationPropertyAsync(commonOptions, c => c.OptionsReloadInSeconds, "SYSTEM");
            await ucp.WriteConfigurationPropertyAsync(commonOptions, c => c.SmsGate, "SYSTEM");

            var database = services.GetRequiredService<DbContext>() as TestApplicationDatabaseContext;
            var dbSettings = database!.ApplicationUserSettings.ToList();
            dbSettings.Should().NotBeEmpty();
            dbSettings.FirstOrDefault(d => d.Key == "Common:OptionsReloadInSeconds")?.Value.Should().NotBeNull().And.Be("888");


            var refreshedConf = lcp.GetConfiguration<CommonConfiguration>();
            refreshedConf.Should().NotBeNull();
            //refreshedConf.OptionsReloadInSeconds.Should().Be(888);
            refreshedConf.SmsGate.PhoneCountryCodes[0].Should().Be("1");
            refreshedConf.SmsGate.PhoneCountryCodes[1].Should().Be("2");

        }

        [Fact]
        public async Task WritingOptionAsProperty_Test()
        {
            var host = Host<TestApplicationDatabaseContext>(useSqlServerConfigurationProvider: true);

            var services = host.Services;

            var ucp = services.GetRequiredService<IUserConfigurationProvider>();
            var lcp = services.GetRequiredService<ILiveConfigurationProvider>();

            var config = lcp.GetConfiguration<TestConfiguration>();

            config.Addresses = new List<ConfigurationAddress> {
                new ConfigurationAddress { City = "Brno", Zip = "000", Favs = new(){ "1", "2" } },
                new ConfigurationAddress { City = "Ropice", Zip = "555" },
                new ConfigurationAddress { City = "Praha", Zip = "111aa" ,
                Description = new ConfigurationAddressMetadataDescription{ Created = "Today" },
                Metadata = new(){
                    new ConfigurationAddressMetadata
                    {
                         Description = new ConfigurationAddressMetadataDescription{ Created = "Now" },
                         Population = 45,
                         QoL = 13
                    },
                    new ConfigurationAddressMetadata
                    {
                         Population = 999,
                         QoL = -555
                    }}},
            };
            config.Delay = 9999;
            config.Settings = new TestSettings { Age = 4 };
            config.LongData = -1;
            config.Addresses[2].Description = new();

            //config.Addresses = null!;

            await ucp.WriteConfigurationPropertyAsync(config, c => c.Addresses[2].Description, "SYSTEM");

            //check
            var database = services.GetRequiredService<TestApplicationDatabaseContext>();

            var dbSettings = database!.ApplicationUserSettings.ToList();

            dbSettings.Should().NotBeEmpty();

            foreach (var settings in dbSettings)
            {
                testOutputHelper.WriteLine($"Key : {settings.Key}, value : {settings.Value}");
            }

            return;

            var refreshedConf = lcp.GetConfiguration<TestConfiguration>();
            refreshedConf.Addresses[2].Metadata[0].Description.Created.Should().Be("Now");

            refreshedConf.Addresses[2].Metadata[0] = default;
            await ucp.WriteConfigurationPropertyAsync(refreshedConf, c => c.Addresses[2], "SYSTEM");


            database = services.GetRequiredService<TestApplicationDatabaseContext>();
            dbSettings = database!.ApplicationUserSettings.ToList();

            foreach (var settings in dbSettings)
            {
                testOutputHelper.WriteLine($"Key : {settings.Key}, value : {settings.Value}");

            }

            //Read typed deep level
            var refreshedTestConf = lcp.GetConfiguration<TestConfiguration>();

            refreshedTestConf.Addresses[2].Zip.Should().Be("111aa");

            refreshedTestConf.Addresses[1].Zip = "Zip";
            ucp.WriteConfigurationPropertyAsync(refreshedTestConf, c => c.Addresses[1], "SYSTEM");

            database = services.GetRequiredService<TestApplicationDatabaseContext>();
            dbSettings = database!.ApplicationUserSettings.ToList();

            foreach (var settings in dbSettings)
            {
                testOutputHelper.WriteLine($"Key : {settings.Key}, value : {settings.Value}");
            }
        }
    }
}
