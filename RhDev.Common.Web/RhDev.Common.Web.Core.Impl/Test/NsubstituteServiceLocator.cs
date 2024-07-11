using NSubstitute;
using StructureMap.AutoMocking;
using System;
using System.Linq;

namespace RhDev.Common.Web.Core.Test._Setup
{
    public class NSubstituteServiceLocator : ServiceLocator
    {
        private Func<Type, object> _typeFactory;

        public NSubstituteServiceLocator()
        {
            var subsType = typeof(Substitute);
            var method = subsType.GetMethods().First(x => x.ContainsGenericParameters && x.GetGenericArguments().Length == 1);
            _typeFactory = typeToMock => method.MakeGenericMethod(typeToMock).Invoke(null, new object[] { null });
        }
        public T PartialMock<T>(params object[] args) where T : class
        {
            return (T)Substitute.ForPartsOf<T>();
        }

        public object Service(Type serviceType)
        {
            return _typeFactory(serviceType);
        }

        public T Service<T>() where T : class
        {
            return (T)Service(typeof(T));
        }

    }
}
