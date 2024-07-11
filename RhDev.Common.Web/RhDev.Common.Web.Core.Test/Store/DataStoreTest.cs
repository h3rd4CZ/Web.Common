using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores.Utils;
using RhDev.Common.Web.Core.Extensions;
using RhDev.Common.Web.Core.Impl.Test;
using RhDev.Common.Web.Core.Test._setup;
using RhDev.Common.Web.Core.Utils;
using RhDev.Customer.Component.Core.Impl.Data;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace RhDev.Common.Web.Core.Test.Store
{
    public class DataStoreTest : IntegrationTestBase
    {
        public DataStoreTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
                
        [Fact]
        public async Task StoreFactoryGenerate_CommonStore_ClientStore_UsingMock_DataPagingTest()
        {
            RegisterLongEfInMemoryMock(1000, "longDatabase");

            var host = Host();

            var services = host.Services;

            var storeFactory = services.GetRequiredService<IDataStoreAcessRepositoryFactory>();

            var clientDataStore = storeFactory.GetDomainQueryableStoreRepository<ITestApplicationDataStore>(Caching.RepositoryCacheStrategy.ReadOnly);

            using (TestMeasureTimeLogger.Start(testOutputHelper!))
            {

                var nextDataCount = 0;
                int page = 0;
                do
                {
                    var pageSize = 30;

                    var skip = pageSize * page;

                    var clientAppStoreData
                        = await clientDataStore.ReadAsync(d => true, orderByDescending: d => d.Id, paging: (skip, pageSize), asNoTracking: true);

                    var data = clientAppStoreData.Data;
                    var totalItems = clientAppStoreData.TotalItems;

                    nextDataCount = data.Count;

                    testOutputHelper.WriteLine($"Batch NR : {page}");
                    testOutputHelper.WriteLine($"Total items : {totalItems}");

                    //foreach (var item in data) testOutputHelper!.WriteLine($"Item : {item.Id}");

                    page++;

                } while (nextDataCount > 0);
            }
        }

        [Fact]
        public async Task StoreFactoryGenerate_CommonStore_ClientStore_UsingMock_Test()
        {
            RegisterEfInMemoryMock();

            var host = Host();

            var services = host.Services;

            var storeFactory = services.GetRequiredService<IDataStoreAcessRepositoryFactory>();

            var clientDataStore = storeFactory.GetDomainQueryableStoreRepository<ITestApplicationDataStore>();

            var clientAppStoreData = await clientDataStore.ReadAllAsync();

            clientAppStoreData.Should().NotBeNull().And.HaveCount(1);

            var commonStore = storeFactory.GetDomainQueryableStoreRepository<IUserConfigurationDataStore>();

            var dataFromCommonStore = await commonStore.ReadSettingsKeyAsync("Common:BaseUri");

            dataFromCommonStore.Should().NotBeNull();

            dataFromCommonStore.Value.Should().Be("baseuri");
        }

        [Fact]
        public async Task StoreFactoryGenerate_ClientStore_UsingRealDB_Test()
        {
            var host = Host<TestApplicationDatabaseContext>();

            var services = host.Services;

            var dbCtx = services.GetRequiredService<TestApplicationDatabaseContext>();

            await dbCtx.Database.EnsureDeletedAsync();
            await dbCtx.Database.EnsureCreatedAsync();

            var storeFactory = services.GetRequiredService<IDataStoreAcessRepositoryFactory>();

            var storeFromClient = storeFactory.GetDomainQueryableStoreRepository<ITestApplicationDataStore>();

            var data = await storeFromClient.ReadAllAsync();

            data.Should().HaveCount(0);

            await storeFromClient.CreateAsync(new ApplicationUserSettings { Changed = DateTime.Now, ChangedBy = "SYSTEM", Key = "key", Value = "value" });

            data = await storeFromClient.ReadAllAsync();

            data.Should().HaveCount(1);
        }

        [Fact]
        public async Task StoreFactoryGenerate_CommonStore_UsingRealDB_Test()
        {
            var host = Host<TestApplicationDatabaseContext>();

            var services = host.Services;

            var dbCtx = services.GetRequiredService<TestApplicationDatabaseContext>();

            await dbCtx.Database.EnsureDeletedAsync();
            await dbCtx.Database.EnsureCreatedAsync();

            var storeFactory = services.GetRequiredService<IDataStoreAcessRepositoryFactory>();

            var storeFromCommon = storeFactory.GetDomainQueryableStoreRepository<IDayOffDataStore>();

            var data = await storeFromCommon.ReadAllAsync();

            data.Should().HaveCountGreaterThan(0);

            var xMassDay = await storeFromCommon.ReadAsync(d => d.Day.Month == 12 && d.Day.Day == 24);

            xMassDay.Should().HaveCount(1);

            xMassDay.First().Repeat.Should().Be(true);

            testOutputHelper!.WriteLine(xMassDay.First().Title);
        }



        void RegisterEfInMemoryMock(string? databaseName = default)
        {
            RegisterEfInMemoryMock<TestApplicationDatabaseContext, DbContext>(ctx =>
            {
                ctx.AddRange(new List<ApplicationUserSettings>
                {
                    new ApplicationUserSettings(){ Id = 1, Key = "Common:BaseUri", Value = "baseuri", Changed = DateTimeOffset.Now, ChangedBy = "SYSTEM"}
                });

                ctx.SaveChanges();
            }, databaseName);
        }

        void RegisterLongEfInMemoryMock(int countData, string? databaseName = default)
        {
            RegisterEfInMemoryMock<TestApplicationDatabaseContext, DbContext>(ctx =>
            {
                ctx.AddRange(Enumerable.Range(1, countData).Select(r =>
                    new ApplicationUserSettings() { Id = r, Key = "Common:BaseUri", Value = $"baseuri_{r}", Changed = DateTimeOffset.Now, ChangedBy = "SYSTEM" }
                ));

                ctx.SaveChanges();
            }, databaseName);
        }
    }
}
