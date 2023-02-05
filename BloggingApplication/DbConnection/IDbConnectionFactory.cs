using Microsoft.Data.SqlClient;

namespace BloggingApplication.DbConnection
{
    public interface IDbConnectionFactory
    {
        public SqlConnection GetDefaultConnection();
    }
}
