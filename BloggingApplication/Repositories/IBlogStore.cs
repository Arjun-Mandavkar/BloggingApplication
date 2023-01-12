using BloggingApplication.Models;
using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Repositories
{
    public interface IBlogStore<TBlog>
    {
        public Task<TBlog> GetByIdAsync(int blogId);
        public Task<IEnumerable<TBlog>> GetAllAsync();
        public Task<Blog> CreateAsync(TBlog blog);
        public Task<IdentityResult> UpdateAsync(TBlog blog);
        public Task<IdentityResult> DeleteAsync(TBlog blog);
    }
}
