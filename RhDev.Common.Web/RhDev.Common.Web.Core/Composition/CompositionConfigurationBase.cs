using Lamar;
using Microsoft.Extensions.DependencyInjection;
using static Lamar.ServiceRegistry;

namespace RhDev.Common.Web.Core.Composition
{
    public abstract class CompositionConfigurationBase
    {
        protected readonly Container container;

        protected ServiceRegistry Configuration { get; private set; }

        protected CompositionConfigurationBase(ServiceRegistry configuration, Container container)
        {
            Configuration = configuration;
            this.container = container;
        }

        public abstract void Apply();

        protected void ConfigureAsTransient<TService>() where TService : class
        {
            Configuration.AddTransient<TService>();
        }

        protected void ConfigureAsSingleton(Type type, Type implType)
        {
            Configuration.AddSingleton(type, implType);
        }

        protected void ConfigureAsSingleton<TService, TImpl>()
        {
            Configuration.AddSingleton(typeof(TService), typeof(TImpl));
        }

        protected void ConfigureAsSingleton<TService>() where TService : class
        {
            Configuration.AddSingleton<TService>();
        }

        protected void ConfigureAsTransient(Type type, Type implType)
        {
            Configuration.AddTransient(type, implType);
        }

        protected void ConfigureAsScoped(Type type, Type implType)
        {
            Configuration.AddScoped(type, implType);
        }

        protected InstanceExpression<TService> For<TService>() where TService : class
        {
            return Configuration.For<TService>();
        }

        protected DescriptorExpression For(Type pluginType)
        {
            return Configuration.For(pluginType);
        }

        protected void SetInjectbale<T>() where T : class
        {
            Configuration.Injectable<T>();
        }
    }
}
