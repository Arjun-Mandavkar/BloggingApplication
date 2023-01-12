using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Services
{
    public interface IUserService
    {
        public Task<UserInfoDto> Register(RegisterUserDto dto);
        public Task<ApplicationUser> FindByEmail(string email);
        public bool IsPasswordCorrect(ApplicationUser user, string password);
        public Task<RoleEnum> GetRole(ApplicationUser user);

        /*--------------------- User CRUD -----------------------*/
        public Task<UserInfoDto> GetByEmail(string email);
        public Task<UserInfoDto> GetById(string userId);
        public Task<UserInfoDto> UpdateUser(ApplicationUser detachedUser);
        public Task<UserInfoDto> DeleteUser(int UserId);

        /*------------------- Dto Mapping Methods ---------------*/
        public Task<ApplicationUser> RegisterUserDtoToApplicationUserEntity(RegisterUserDto dto);
        public Task<UserInfoDto> ApplicationUserEntityToUserInfoDto(ApplicationUser entity, bool isTokenRequired);
    }
}
