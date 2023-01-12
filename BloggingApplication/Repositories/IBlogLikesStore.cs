using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Repositories
{
    public interface IBlogLikesStore<TBLog,TUser>
    {
        public Task<bool> IsLikedAsync(TBLog blog, TUser user);
        public Task<IdentityResult> LikeAsync(TBLog blog, TUser user);
        public Task<IdentityResult> UndoLikeAsync(TBLog blog, TUser user);
    }
}
