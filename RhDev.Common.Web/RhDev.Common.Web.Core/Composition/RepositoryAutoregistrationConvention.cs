using JasperFx.Core.TypeScanning;
using Lamar;
using Lamar.Scanning.Conventions;
using RhDev.Common.Web.Core.DataStore;

namespace RhDev.Common.Web.Core.Composition
{
    public class RepositoryAutoregistrationConvention : IRegistrationConvention
    {
        private static readonly Type BaseServiceInterface = typeof(IService);
        private static readonly Type RepositoryRegisteredServiceInterface = typeof(IDataStoreRepository);

        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            foreach (var implementation in types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed))
            {
                if (!IsRepositoryImplementation(implementation))
                    continue;

                var serviceInterfaces = implementation.GetInterfaces().Where(IsNotBaseServiceInterface);

                foreach (Type serviceInterface in serviceInterfaces)
                    RegisterImplementationForServiceInterface(services, implementation, serviceInterface);
            }
        }

        private static bool IsRepositoryImplementation(Type implementation)
        {
            return RepositoryRegisteredServiceInterface.IsAssignableFrom(implementation) && !implementation.IsAbstract;
        }

        private static bool IsNotBaseServiceInterface(Type serviceInterface)
        {
            return serviceInterface != BaseServiceInterface && serviceInterface != BaseServiceInterface;
        }

        private static void RegisterImplementationForServiceInterface(ServiceRegistry registry, Type implementation, Type serviceInterface)
        {
            registry.For(serviceInterface).Use(implementation);
        }
    }
}
