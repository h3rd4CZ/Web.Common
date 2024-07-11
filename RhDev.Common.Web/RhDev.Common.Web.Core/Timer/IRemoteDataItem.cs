using System.Data.SqlClient;

namespace RhDev.Common.Web.Core.Timer
{
    public interface IRemoteDataItem
    {
        void FillFrom(SqlDataReader dataReader);
        SqlCommand CreateDataCommand(SqlConnection connection, string entityName);
    }
}
