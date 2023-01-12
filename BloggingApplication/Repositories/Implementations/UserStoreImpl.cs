using BloggingApplication.Models;
using BloggingApplication.Services;
using Microsoft.AspNetCore.Identity;
using Dapper;
using BloggingApplication.CustomExceptions;

namespace BloggingApplication.Repositories.Implementations
{
    public class UserStoreImpl : IUserStore<ApplicationUser>
    {
        private IDbConnectionFactory _connectionFactory { get; }

        public UserStoreImpl(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string query1 = $@"INSERT INTO [Users]
                               ([Email], [Name], [PasswordHash])
                               VALUES (@{nameof(ApplicationUser.Email)},@{nameof(ApplicationUser.Name)},@{nameof(ApplicationUser.PasswordHash)});
                               SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync(cancellationToken);
                user.Id = await connection.ExecuteScalarAsync<int>(query1, user);
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string query = $"DELETE FROM [USERS] WHERE Id = @{nameof(ApplicationUser.Id)}";
            int? rowsAffected = null;

            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync(cancellationToken);
                rowsAffected = await connection.ExecuteAsync(query, user);
            }

            if (rowsAffected == 1)
                return IdentityResult.Success;
            else
                return IdentityResult.Failed();
        }

        public void Dispose()
        {
            //Nothing to dispose.
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int Id = int.Parse(userId);
            ApplicationUser user = null;
            string query = $"SELECT * FROM [Users] WHERE Id = @UserId";
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync(cancellationToken);
                user = await connection.QuerySingleOrDefaultAsync<ApplicationUser>(query, new { UserId = Id });
            }
            return user;
        }

        public async Task<ApplicationUser> FindByNameAsync(string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ApplicationUser user = null;
            string query = "SELECT * FROM [Users] WHERE Email = @Email";
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync(cancellationToken);
                user = await connection.QuerySingleAsync<ApplicationUser>(query, new { Email = email });
            }
            return user;
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
