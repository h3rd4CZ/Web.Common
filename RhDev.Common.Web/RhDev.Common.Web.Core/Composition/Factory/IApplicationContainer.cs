using System;

namespace RhDev.Common.Web.Core.Composition.Factory
{
    public interface IApplicationContainer
    {
        T GetInstance<T>();
        T TryGetInstance<T>();
        object GetInstance(Type type);
        void BuildUp(object target);
    }
}
