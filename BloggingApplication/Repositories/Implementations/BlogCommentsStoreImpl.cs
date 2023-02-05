using BloggingApplication.DbConnection;
using BloggingApplication.Models;
using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading;

namespace BloggingApplication.Repositories.Implementations
{
    public class BlogCommentsStoreImpl : IBlogCommentsStore<BlogComment>
    {
        private IDbConnectionFactory _connectionFactory { get; }
        public BlogCommentsStoreImpl(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<BlogComment> CreateAsync(BlogComment comment)
        {
            string query = $@"INSERT INTO [BlogComments]
                              ([UserId],[BlogId],[Text],[TimeStamp],[UserName],[IsUserExists])
                              VALUES(@{nameof(BlogComment.UserId)},
                                     @{nameof(BlogComment.BlogId)},
                                     @{nameof(BlogComment.Text)},
                                     @{nameof(BlogComment.TimeStamp)},
                                     @{nameof(BlogComment.UserName)},
                                     @{nameof(BlogComment.IsUserExists)});
                              SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                comment.Id = await connection.ExecuteScalarAsync<int>(query, comment);
            }
            return comment;
        }

        public async Task<IdentityResult> DeleteAsync(BlogComment comment)
        {
            string query = $@"DELETE FROM [BlogComments] WHERE Id = @{nameof(BlogComment.Id)}";

            int? rowsAffected = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                rowsAffected = await connection.ExecuteAsync(query, comment);
            }

            if (rowsAffected == 1)
                return IdentityResult.Success;
            else
                return IdentityResult.Failed();
        }

        public async Task<IEnumerable<BlogComment>> GetAllFromBlogAsync(int blogId)
        {
            string query = $@"SELECT * FROM [BlogComments] WHERE BlogId = @BlogId";
            IEnumerable<BlogComment> list = new List < BlogComment >();
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                list = await connection.QueryAsync<BlogComment>(query, new {BlogId = blogId});
            }
            return list;
        }

        public async Task<IEnumerable<BlogComment>> GetAllFromUserAsync(int userId)
        {
            string query = $@"SELECT * FROM [BlogComments] WHERE UserId = @UserId";
            IEnumerable<BlogComment> list = new List<BlogComment>();
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                list = await connection.QueryAsync<BlogComment>(query, new { UserId = userId });
            }
            return list;
        }

        public async Task<BlogComment> GetAsync(int id)
        {
            string query = $@"SELECT * FROM [BlogComments] WHERE Id = @Id";
            BlogComment comment = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                comment = await connection.QuerySingleOrDefaultAsync<BlogComment>(query, new { Id = id });
            }
            return comment;
        }
        public async Task<IdentityResult> UpdateAsync(BlogComment comment)
        {
            string query = $@"UPDATE [BlogComments] SET
                              [Text]=@{nameof(BlogComment.Text)}
                              WHERE Id = @{nameof(BlogComment.Id)};";

            int? rowsAffected = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                rowsAffected = await connection.ExecuteAsync(query, comment);
            }

            if (rowsAffected == 1)
                return IdentityResult.Success;
            else
                return IdentityResult.Failed();
        }

        public async Task<IdentityResult> SetIsUserExistsFalse(int userId)
        {
            string query = $@"UPDATE [BlogComments]
                              SET [IsUserExists]=@{nameof(BlogComment.IsUserExists)},[UserId]=0
                              WHERE [UserId]=@{nameof(BlogComment.UserId)};";

            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(query, new BlogComment { UserId = userId, IsUserExists = false });
            }
            return IdentityResult.Success;
        }

        
    }
}
