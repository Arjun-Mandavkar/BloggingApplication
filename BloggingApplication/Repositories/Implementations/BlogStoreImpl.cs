using BloggingApplication.DbConnection;
using BloggingApplication.Models;
using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Threading;

namespace BloggingApplication.Repositories.Implementations
{
    public class BlogStoreImpl : IBlogStore<Blog>
    {
        private IDbConnectionFactory _connectionFactory { get; }
        public BlogStoreImpl(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Blog> GetByIdAsync(int blogId)
        {
            string query = $"SELECT * FROM [Blogs] WHERE Id = @BlogId";
            Blog blog = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                blog = await connection.QuerySingleOrDefaultAsync<Blog>(query, new { BlogId = blogId });
            }
            return blog;
        }

        public async Task<IEnumerable<Blog>> GetAllAsync()
        {
            string query = $"SELECT * FROM [Blogs]";
            IEnumerable<Blog> blogs = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                blogs = await connection.QueryAsync<Blog>(query);
            }
            return blogs;
        }

        public async Task<Blog> CreateAsync(Blog blog)
        {
            string query = $@"INSERT INTO [Blogs]
                              ([Title], [Content], [Likes])
                              VALUES (@{nameof(Blog.Title)},@{nameof(Blog.Content)},@{nameof(Blog.Likes)});
                              SELECT CAST(SCOPE_IDENTITY() as int)";
            using(var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                blog.Id = await connection.ExecuteScalarAsync<int>(query, blog);
            }
            return blog;
        }

        public async Task<IdentityResult> UpdateAsync(Blog blog)
        {
            string query = $@"UPDATE [Blogs]
                              SET [Title] = @{nameof(Blog.Title)}, [Content] = @{nameof(Blog.Content)}
                              WHERE Id = @{nameof(Blog.Id)};
                              ";
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                blog.Id = await connection.ExecuteAsync(query, blog);
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(int blogId)
        {
            string query = $@"DELETE FROM [Blogs]
                              WHERE Id = @BlogId;
                              ";
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(query, new {BlogId = blogId});
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> IncrementLike(int blogId)
        {
            string query = $@"UPDATE [Blogs] SET [Likes] = [Likes] + 1 WHERE [Id] = @BlogId";
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(query, new { BlogId = blogId});
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DecrementLike(int blogId)
        {
            string query = $@"UPDATE [Blogs] SET [Likes] = [Likes] - 1 WHERE [Id] = @BlogId";
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(query, new { BlogId = blogId });
            }
            return IdentityResult.Success;
        }
    }
}
