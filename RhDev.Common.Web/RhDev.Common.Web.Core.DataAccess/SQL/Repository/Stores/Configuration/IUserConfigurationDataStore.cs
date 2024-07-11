using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores
{
    public interface IUserConfigurationDataStore : IStoreRepository<ApplicationUserSettings>
    {
        Task<ApplicationUserSettings> ReadSettingsKeyAsync(string key);
    }
}
