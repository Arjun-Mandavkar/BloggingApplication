using Microsoft.Data.SqlClient;

namespace BloggingApplication.Services
{
    public interface IDbConnectionFactory
    {
        public SqlConnection GetDefaultConnection();
    }
}
