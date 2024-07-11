using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core.Utils;
using System.Data.SqlClient;

namespace RhDev.Common.Web.Core.Timer
{
    public abstract class SqlDataProviderBase
    {
        private readonly ILogger logger;

        public SqlDataProviderBase(ILogger logger)
        {
            this.logger = logger;
        }

        protected async Task<IList<TItem>> ReadRawData<TItem>(string connectionString, string query) where TItem : IRemoteDataItem, new()
        {
            Guard.StringNotNullOrWhiteSpace(connectionString, nameof(connectionString));
            Guard.StringNotNullOrWhiteSpace(query, nameof(query));

            return await UseSqlConnectionAndReturn(async connection =>
            {
                var cdm = new SqlCommand(query, connection);

                var data = await cdm.ExecuteReaderAsync();

                var items = new List<TItem>();

                while (data.Read())
                {
                    var dataItem = new TItem();

                    dataItem.FillFrom(data);

                    items.Add(dataItem);
                }

                return items;

            }, connectionString);
        }

        protected async Task CreateData<TItem>(string connectionString, TItem item, string entityName) where TItem : IRemoteDataItem, new()
        {
            Guard.StringNotNullOrWhiteSpace(connectionString, nameof(connectionString));

            await UseSqlConnection(async connection =>
            {
                var cmd = item.CreateDataCommand(connection, entityName);

                logger.LogInformation($"Executing create data command for entity : {entityName} with command : {cmd?.CommandText}");

                await cmd.ExecuteNonQueryAsync();

            }, connectionString);
        }

        private async Task<TData> UseSqlConnectionAndReturn<TData>(Func<SqlConnection, Task<TData>> a, string connectionString)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();

                return await a(sqlConnection);
            }
        }

        private async Task UseSqlConnection(Func<SqlConnection, Task> a, string connectionString)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();

                await a(sqlConnection);
            }
        }
    }
}
