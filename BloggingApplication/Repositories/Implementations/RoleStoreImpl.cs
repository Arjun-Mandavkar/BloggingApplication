using BloggingApplication.Models;
using BloggingApplication.Services;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Repositories.Implementations
{
    public class RoleStoreImpl : IRoleStore<IdentityRole>
    {
        private IDbConnectionFactory _connectionFactory { get; }
        public RoleStoreImpl(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //Nothing to dispose.
        }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IdentityRole role = null;
            string query = "SELECT * FROM [Roles] WHERE Id = @Id";
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync(cancellationToken);
                role = await connection.QuerySingleOrDefaultAsync<IdentityRole>(query, new { Id = Int32.Parse(roleId) });
            }
            return role;
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IdentityRole role = null;
            string query = "SELECT * FROM [Roles] WHERE Name = @Name";
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync(cancellationToken);
                role = await connection.QuerySingleOrDefaultAsync<IdentityRole>(query, new { Name = normalizedRoleName });
            }
            return role;
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
