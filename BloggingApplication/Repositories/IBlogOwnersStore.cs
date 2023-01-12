using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Repositories
{
    public interface IBlogOwnersStore<TBlogOwner>
    {
        public Task<bool> IsOwner(TBlogOwner blog);
        public Task<IdentityResult> AssignOwner(TBlogOwner blog);
        public Task<IdentityResult> RevokeOwner(TBlogOwner blog);
        public Task<TBlogOwner> Get(int userId, int BlogId);
        public Task<IdentityResult> Update(TBlogOwner blog);
        public Task<IdentityResult> SetIsOwnerExistsFalse(int userId);
    }
}
