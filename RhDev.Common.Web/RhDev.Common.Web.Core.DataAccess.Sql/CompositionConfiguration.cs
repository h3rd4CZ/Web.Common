using Lamar;
using RhDev.Common.Web.Core.Caching;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.Sql.Repository;
using RhDev.Common.Web.Core.DataAccess.Sql.Repository.Stores;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using RhDev.Common.Web.Core.DataStore;

namespace RhDev.Common.Web.Core.DataAccess.Sql
{
    public class CompositionConfiguration : ConventionConfigurationBase
    {

        public CompositionConfiguration(ServiceRegistry configuration, Container container) : base(configuration, container)
        {
        }

        public override void Apply()
        {
            base.Apply();

            SetInjectbale<RepositoryCacheSettings>();

            ConfigureAsTransient(typeof(IDatabaseAccessRepositoryFactory<>), typeof(DatabaseAccessRepositoryFactory<>));

            For(typeof(IDynamicDataStoreRepository<>)).Use(typeof(DynamicDataStoreRepository<>));

            For<DataStoreConstructor>().Use(ctx =>
            {   
                return (type, cs) =>
                {
                    var repositoryType = DataStoreAcessRepositoryFactory.GetRepositoryImplementationDomainInterface(type);

                    return (IDataStoreRepository)InjectAndGet<RepositoryCacheSettings, object, object, object, object>(repositoryType, cs, null, null, null, null);
                };
            });
        }
    }
}
