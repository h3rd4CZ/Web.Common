using JasperFx.Core.TypeScanning;
using Lamar;
using Lamar.Scanning.Conventions;

namespace RhDev.Common.Web.Core.Composition
{
    public class ServiceAutoRegistrationConvention : IRegistrationConvention
    {
        private static readonly Type BaseServiceInterface = typeof(IService);
        private static readonly Type BaseAutoRegisteredServiceInterface = typeof(IAutoregisteredService);

        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            foreach (var implementation in types.FindTypes(TypeClassification.Open | TypeClassification.Concretes | TypeClassification.Closed))
            {
                if (!IsServiceImplementation(implementation))
                    continue;

                var serviceInterfaces = implementation.GetInterfaces().Where(IsNotBaseServiceInterface);

                foreach (Type serviceInterface in serviceInterfaces)
                    RegisterImplementationForServiceInterface(services, implementation, serviceInterface);
            }
        }

        private static bool IsServiceImplementation(Type implementation)
        {
            return BaseAutoRegisteredServiceInterface.IsAssignableFrom(implementation) && !implementation.IsAbstract;
        }

        private static bool IsNotBaseServiceInterface(Type serviceInterface)
        {
            return serviceInterface != BaseServiceInterface && serviceInterface != BaseAutoRegisteredServiceInterface;
        }

        private static void RegisterImplementationForServiceInterface(ServiceRegistry registry, Type implementation, Type serviceInterface)
        {
            registry.For(serviceInterface).Use(implementation);
        }
    }
}
