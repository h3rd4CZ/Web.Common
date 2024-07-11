using RhDev.Common.Web.Core.Composition;

namespace RhDev.Customer.Component.Core.Impl
{
    public interface IFoo : IAutoregisteredService
    {
        void Boo();
    }
}
