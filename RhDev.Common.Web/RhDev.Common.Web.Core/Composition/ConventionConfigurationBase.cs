using Lamar;
using RhDev.Common.Web.Core.Utils;
using System;
using System.Collections.Generic;

namespace RhDev.Common.Web.Core.Composition
{
    public class ConventionConfigurationBase : CompositionConfigurationBase
    {
        protected virtual IList<Type> RuntimeInjectedServices { get; } = new List<Type> { };

        protected ConventionConfigurationBase(ServiceRegistry configuration, Container container)
            : base(configuration, container)
        {

        }

        public override void Apply()
        {
            Configuration.Scan(scanner =>
            {
                scanner.Assembly(GetType().Assembly);
                scanner.Convention<ServiceAutoRegistrationConvention>();
                scanner.Convention<RepositoryAutoregistrationConvention>();
            });
        }

        protected object InjectAndGet<T1InjectedService, T2InjectedService, T3InjectedService, T4InjectedService, T5InjectedService>
            (
            Type service, T1InjectedService injectableObject1, T2InjectedService injectableObject2,
            T3InjectedService injectableObject3, T4InjectedService injectableObject4,
            T5InjectedService injectableObject5)
        {
            Guard.NotNull(service, nameof(service));
            var nested = container.GetNestedContainer();
                                 
            if (!Equals(null, injectableObject1)) nested.Inject(injectableObject1);
            if (!Equals(null, injectableObject2)) nested.Inject(injectableObject2);
            if (!Equals(null, injectableObject3)) nested.Inject(injectableObject3);
            if (!Equals(null, injectableObject4)) nested.Inject(injectableObject4);
            if (!Equals(null, injectableObject5)) nested.Inject(injectableObject5);

            var s = nested.GetInstance(service);

            return s;
        }

        protected object InjectAndGet<TInjectedService>(Type service, TInjectedService injectableObject)
        {
            Guard.NotNull(injectableObject, nameof(injectableObject));

            var nested = container.GetNestedContainer();
            nested.Inject(injectableObject);
            var s = nested.GetInstance(service);
            //nested.Dispose();
            return s;
        }

        protected TService InjectAndGet<TService, TInjectedService>(TInjectedService injectableObject)
             => (TService)InjectAndGet(typeof(TService), injectableObject);
    }
}
