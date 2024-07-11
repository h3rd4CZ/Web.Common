using Microsoft.Extensions.Hosting;
using RhDev.Common.Web.Core.Impl.Host;
using RhDev.Common.Web.Core.Test._Setup;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using RhDev.Common.Web.Core.Composition.Factory.Definitions;
using Microsoft.Extensions.DependencyInjection;

namespace RhDev.Common.Web.Core.Test
{
    public abstract class IntegrationTestBase
    {
        private IHost host = default!;

        private readonly NSubstituteServiceLocator nSubstituteServiceLocator = new NSubstituteServiceLocator();
        protected readonly ITestOutputHelper? testOutputHelper;

        protected IntegrationTestBase() { }
        public IntegrationTestBase(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        protected TMock GetMock<TMock>() where TMock : class
        {
            return nSubstituteServiceLocator.Service<TMock>();
        }

        protected object GetMock(Type mockType)
        {
            return nSubstituteServiceLocator.Service(mockType);
        }

        protected IHost Host() => Host<DbContext>(default, default, false);
        protected IHost Host(ContainerRegistrationDefinition[] containerRegistrationDefinitions, bool useSqlServerConfigurationProvider = false) => Host<DbContext>(
            containerRegistrationDefinitions,
            default,
            useSqlServerConfigurationProvider: useSqlServerConfigurationProvider);

        protected IHost Host(
            string[] envVarPrefixes = default!,
            ContainerRegistrationDefinition[]? containerRegistrationDefinitions = default,
            Action<HostBuilderContext, IServiceCollection>? serviceConfiguration = default,
            bool useSqlServerConfigurationProvider = false) 
            => Host<DbContext>(containerRegistrationDefinitions, envVarPrefixes, useSqlServerConfigurationProvider : useSqlServerConfigurationProvider, serviceConfiguration);

        protected IHost Host<TDbContext>(
            ContainerRegistrationDefinition[]? containerRegistrationDefinitions = default,
            string[] envVarPrefixes = default!,
            bool useSqlServerConfigurationProvider = false,
            Action<HostBuilderContext, IServiceCollection>? serviceConfiguration = default,
            bool useDbContextFactory = false) where TDbContext : DbContext
        {
            Init<TDbContext>(containerRegistrationDefinitions, useSqlServerConfigurationProvider: useSqlServerConfigurationProvider, useDbContextFactory, serviceConfiguration: serviceConfiguration, envVarPrefixes);

            return host;
        }

        IDictionary<Type, Func<IServiceProvider, object>> defaultMocks = new Dictionary<Type, Func<IServiceProvider, object>>();

        protected void RegisterMock(Type type, Func<IServiceProvider, object> mock)
        {
            if (!defaultMocks.ContainsKey(type)) defaultMocks.Add(type, mock);
        }

        protected void RegisterMock(Type type, object mock)
        {
            if (!defaultMocks.ContainsKey(type)) defaultMocks.Add(type, p => mock);
        }

        protected void RegisterEfInMemoryMock<TContext>(Action<TContext> seedAction) where TContext : DbContext
            => RegisterEfInMemoryMock<TContext, TContext>(seedAction);
        
        protected void RegisterEfInMemoryMock<TContext, TSourceContext>(Action<TContext> seedAction, string? databaseName = default) where TContext : DbContext where TSourceContext : DbContext
        {
            var options = new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(databaseName: databaseName ?? "testdatabase")
                .Options;

            var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), options)!;

            seedAction(dbContext);

            ///To use DBContext in common class library
            RegisterMock(typeof(TSourceContext), dbContext);

            ///To use implemented DBContext in client solution 
            RegisterMock(typeof(TContext), dbContext);
        }

        private void Init<TDbContext>(
            ContainerRegistrationDefinition[]? registrationDefinitions,
            bool useSqlServerConfigurationProvider,
            bool useDbContextFactory,
            Action<HostBuilderContext, IServiceCollection>? serviceConfiguration = default,
            string[] envVarPrefixes = default!) where TDbContext : DbContext
        {
            host = ApplicationHostBuilder.CreateMinimalForTest<TDbContext>(testOutputHelper: testOutputHelper,
                useSqlServerConfigurationProvider: useSqlServerConfigurationProvider,
                useDbContextFactory : useDbContextFactory,
                serviceConfiguration : serviceConfiguration,
                registrationDefinitions: registrationDefinitions, envVarPrefixes: envVarPrefixes, mocks: defaultMocks);

        }
    }
}
