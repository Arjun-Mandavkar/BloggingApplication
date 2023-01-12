using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Repositories
{
    public interface IBlogEditorsStore<TBlog,TUser>
    {
        public Task<bool> IsEditor(TBlog blog, TUser user);
        public Task<IdentityResult> AssignEditor(TBlog blog, TUser user);
        public Task<IdentityResult> RevokeEditor(TBlog blog, TUser user);
    }
}
