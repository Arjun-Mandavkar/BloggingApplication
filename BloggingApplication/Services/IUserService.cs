using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;

namespace BloggingApplication.Services
{
    public interface IUserService
    {
        public Task<UserInfoDto> Register(RegisterUserDto dto);
        public Task<UserInfoDto> Login(LoginUserDto dto);

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
