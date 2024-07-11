using Lamar;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Composition.Factory
{
    public class ApplicationContainer : IApplicationContainer
    {
        private readonly IContainer container;

        public ApplicationContainer(IContainer container)
        {
            Guard.NotNull(container, nameof(container));

            this.container = container;
        }

        public void BuildUp(object target)
        {
            //container.BuildUp(target);
        }

        public T GetInstance<T>() => container.GetInstance<T>();
        public T TryGetInstance<T>() => container.TryGetInstance<T>();

        public object GetInstance(Type type)
        {
            Guard.NotNull(type, nameof(type));

            return container.GetInstance(type);
        }
    }
}
