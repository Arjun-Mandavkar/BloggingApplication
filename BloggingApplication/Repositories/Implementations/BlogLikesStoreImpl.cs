using BloggingApplication.DbConnection;
using BloggingApplication.Models;
using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Runtime.InteropServices;
using System.Threading;

namespace BloggingApplication.Repositories.Implementations
{
    public class BlogLikesStoreImpl : IBlogLikesStore<Blog, ApplicationUser>
    {
        private IDbConnectionFactory _connectionFactory { get; }
        public BlogLikesStoreImpl(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> IsLikedAsync(Blog blog, ApplicationUser user)
        {
            string query = $@"SELECT [BlogId],[UserId] FROM [BlogLikes]
                              WHERE UserId = @{nameof(BlogLike.UserId)} 
                              AND BlogId = @{nameof(BlogLike.BlogId)}";

            BlogLike like = new BlogLike { BlogId = blog.Id, UserId = user.Id };

            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                BlogLike entry = await connection.QuerySingleOrDefaultAsync<BlogLike>(query, like);
                return (entry == null) ? false : true;
            }
        }

        public async Task<IdentityResult> LikeAsync(Blog blog, ApplicationUser user)
        {
            string query = $@"INSERT INTO [BlogLikes]
                              ([BlogId],[UserId])
                              VALUES(@{nameof(BlogLike.BlogId)},@{nameof(BlogLike.UserId)})";
            BlogLike like = new BlogLike { BlogId=blog.Id,UserId=user.Id};
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(query, like);
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UndoLikeAsync(Blog blog, ApplicationUser user)
        {
            string query = $@"DELETE FROM [BlogLikes]
                              WHERE BlogId = @{nameof(BlogLike.BlogId)}
                              AND UserId = @{nameof(BlogLike.UserId)};";
            BlogLike like = new BlogLike { BlogId = blog.Id, UserId = user.Id };

            int? rowsAffected = null;
            using (var connection = _connectionFactory.GetDefaultConnection())
            {
                await connection.OpenAsync();
                rowsAffected = await connection.ExecuteAsync(query, like);
            }
            if (rowsAffected == 1)
                return IdentityResult.Success;
            else
                throw new InvalidOperationException("Invalid 'Undo Like' request.");
        }
    }
}
