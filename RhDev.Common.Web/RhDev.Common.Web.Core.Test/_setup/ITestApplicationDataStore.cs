using RhDev.Common.Web.Core.DataAccess;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;

namespace RhDev.Common.Web.Core.Test._setup
{
    public interface ITestApplicationDataStore : IStoreRepository<ApplicationUserSettings> { }
}
