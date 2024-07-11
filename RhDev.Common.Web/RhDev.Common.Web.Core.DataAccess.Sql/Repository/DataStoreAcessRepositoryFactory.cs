using Microsoft.EntityFrameworkCore.Storage;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.DataAccess.Sql.Repository.Stores.Utils;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataStore;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Repository
{
    public delegate IDataStoreRepository DataStoreConstructor(
                        Type repositoryType, RepositoryCacheSettings cacheStrategy);

    public class DataStoreAcessRepositoryFactory : IDataStoreAcessRepositoryFactory
    {
        private readonly DataStoreConstructor dataStoreConstructor;
        private readonly IAutoRegisterStoreRepository[] autoRegisterStoreRepositories;
        private readonly IServiceProvider serviceProvider;

        public DataStoreAcessRepositoryFactory(
            DataStoreConstructor dataStoreConstructor,
            IAutoRegisterStoreRepository[] autoRegisterStoreRepositories,
            IServiceProvider serviceProvider)
        {
            this.dataStoreConstructor = dataStoreConstructor;
            this.autoRegisterStoreRepositories = autoRegisterStoreRepositories;
            this.serviceProvider = serviceProvider;
        }
        public IStoreRepository<TEntity> GetAutoregisterStoreRepositoryForEntity<TEntity>() where TEntity : class, IDataStoreEntity
        {
            var entityType = typeof(TEntity); 

            var repository = autoRegisterStoreRepositories.FirstOrDefault(
                r =>
                    r.GetType().GetInterfaces().Any(i => i == typeof(IStoreRepository<TEntity>))
                );

            if (Equals(null, repository))
                throw new Exception($"Store repository for type : {entityType} was not found. Please make sure that the repository implements IAutoRegisterStoreRepository interface");

            return (IStoreRepository<TEntity>)repository;
        }

        public TStoreRepository GetDomainQueryableStoreRepositoryUsingNestedContainer<TStoreRepository>(RepositoryCacheStrategy repositoryCacheStrategy = RepositoryCacheStrategy.AlwaysBypass) where TStoreRepository : IDataStoreRepository
        {
            var repository = dataStoreConstructor(typeof(TStoreRepository), RepositoryCacheSettings.ForCacheStrategy(repositoryCacheStrategy));

            Guard.NotNull(repository, nameof(repository));

            return (TStoreRepository)repository;
        }

        public TStoreRepository GetDomainQueryableStoreRepository<TStoreRepository>(RepositoryCacheStrategy repositoryCacheStrategy = RepositoryCacheStrategy.AlwaysBypass) where TStoreRepository : IDataStoreRepository
        {
            var storeType = GetRepositoryImplementationDomainInterface(typeof(TStoreRepository));

            Guard.NotNull(storeType, nameof(storeType),$"There is not {typeof(TStoreRepository).Name} type registered as data store.");
                        
            var ctor = storeType.GetConstructors()
                .FirstOrDefault(c => c.IsPublic && c.GetParameters()?.Length > 0);

            Guard.NotNull(ctor, nameof(ctor), $"Type {storeType} does not contain public constructor with at least 1 argument");

            var ctorParams = ctor.GetParameters();

            var ctorArgObjects = ctorParams.Select(p => p.ParameterType switch
            {
                { } when p.ParameterType == typeof(RepositoryCacheSettings) => RepositoryCacheSettings.ForCacheStrategy(repositoryCacheStrategy),
                _ => serviceProvider.GetService(p.ParameterType)
            }).ToList();

            var storeObject = Activator.CreateInstance(storeType, ctorArgObjects.ToArray());

            return (TStoreRepository)storeObject;
        }

        public static Type GetRepositoryImplementationDomainInterface(Type dataStoreTypeRepositoryInterface)
        {
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith(Constants.SOLUTION_PREFIX_COMMON_LIBS_BASED)))
            {
                var type = ass.GetTypes().FirstOrDefault(t => dataStoreTypeRepositoryInterface.IsAssignableFrom(t) && !t.IsAbstract && !Equals(null, t.BaseType));

                if (!Equals(null, type)) return type;
            }

            return null;
        }
    }
}
