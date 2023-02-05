using BloggingApplication.DbConnection;
using BloggingApplication.Models;
using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Repositories.Implementations
{
    public class BlogEditorsStoreImpl : IBlogEditorsStore<Blog, ApplicationUser>
    {
        private IDbConnectionFactory _connectionFactory { get; }
        public BlogEditorsStoreImpl(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<IEnumerable<int>> Get(int blogId)
        {
            string query = $@"SELECT [UserId]
                              FROM [BlogEditors]
                              WHERE BlogId = @BlogId";
            IEnumerable<int> result = new List<int>();
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                result = await connection.QueryAsync<int>(query, new { BlogId = blogId });
            }
            return result;
        }
        public async Task<bool> IsEditor(Blog blog, ApplicationUser user)
        {
            string query = $@"SELECT [BlogId],[UserId] FROM [BlogEditors]
                              WHERE UserId = @{nameof(BlogEditor.UserId)} 
                              AND BlogId = @{nameof(BlogEditor.BlogId)}";
            BlogEditor entity = new BlogEditor { BlogId = blog.Id,UserId=user.Id };
            BlogEditor entry = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                entry = await connection.QuerySingleOrDefaultAsync<BlogEditor>(query, entity);
            }
            return (entry == null) ? false : true;
        }

        public async Task<IdentityResult> AssignEditor(Blog blog, ApplicationUser user)
        {
            if (await IsEditor(blog,user))
                throw new InvalidOperationException("User is already an editor.");

            string query = $@"INSERT INTO [BlogEditors]
                              ([UserId],[BlogId])
                              VALUES(@{nameof(BlogEditor.UserId)},@{nameof(BlogEditor.BlogId)})";
            
            BlogEditor entity = new BlogEditor { BlogId = blog.Id, UserId = user.Id };

            int? rowsAffected = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                rowsAffected = await connection.ExecuteAsync(query, entity);
            }
            if (rowsAffected == 1)
                return IdentityResult.Success;
            else
                return IdentityResult.Failed();
        }

        public async Task<IdentityResult> RevokeEditor(Blog blog, ApplicationUser user)
        {
            if (!await IsEditor(blog, user))
                throw new InvalidOperationException("User does not have editor role.");

            string query = $@"DELETE FROM [BlogEditors]
                              WHERE UserId = @{nameof(BlogEditor.UserId)} 
                              AND BlogId = @{nameof(BlogEditor.BlogId)}";

            BlogEditor entity = new BlogEditor { BlogId = blog.Id, UserId = user.Id };

            int? rowsAffected = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                rowsAffected = await connection.ExecuteAsync(query, entity);
            }
            if (rowsAffected == 1)
                return IdentityResult.Success;
            else
                return IdentityResult.Failed();
        }
    }
}
