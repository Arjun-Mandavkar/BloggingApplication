using BloggingApplication.DbConnection;
using Microsoft.Data.SqlClient;

namespace BloggingApplication.DbConnection.Implementations
{
    public class DbConnectionFactoryImpl : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public DbConnectionFactoryImpl(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetDefaultConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
