using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Repositories
{
    public interface IUserRolesStore<TRole, TUser>
    {
        /*----- move these 2 methods to RoleStore ----------*/
        public Task<TRole> GetUserSingleRoleAsync(int userId, CancellationToken token);
        public Task<IEnumerable<TRole>> GetUserRolesAsync(TUser user, CancellationToken token);

        /*----- move these 2 methods to UserStore ----------*/
        public Task<IdentityResult> AssignNewAsync(TRole role, TUser user, CancellationToken token);
        public Task<IdentityResult> RevokeRoleAsync(TRole role, TUser user, CancellationToken token);
    }
}
