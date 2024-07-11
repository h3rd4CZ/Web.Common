using RhDev.Common.Web.Core.Composition;

namespace RhDev.Common.Web.Core.DataAccess.Concurrent
{
    public interface IConcurrentDataAccessRepository : IAutoregisteredService
    {
        void UseService<T>(Action a) where T : ISynchronizationContextService;
        Task UseIdentifierAsync(string identifier, Func<Task> f);
    }
}
