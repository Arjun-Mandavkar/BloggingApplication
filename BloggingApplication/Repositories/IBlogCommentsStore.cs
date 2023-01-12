using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Repositories
{
    public interface IBlogCommentsStore<TComment>
    {
        public Task<TComment> GetAsync(int id);
        public Task<TComment> CreateAsync(TComment comment);
        public Task<IdentityResult> UpdateAsync(TComment comment);
        public Task<IdentityResult> DeleteAsync(TComment comment);
        public Task<IEnumerable<TComment>> GetAllFromBlogAsync(int blogId);
        public Task<IEnumerable<TComment>> GetAllFromUserAsync(int userId);
        public Task<IdentityResult> SetIsUserExistsFalse(int userId);
    }
}
