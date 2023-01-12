using BloggingApplication.CustomExceptions;
using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using BloggingApplication.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Transactions;

namespace BloggingApplication.Services.Implementations
{
    public class UserServiceImpl : IUserService
    {
        private IUserStore<ApplicationUser> _userStore;
        private IUserRolesStore<IdentityRole, ApplicationUser> _userRoleStore;
        private IRoleStore<IdentityRole> _roleStore;
        private IBlogOwnersStore<BlogOwner> _blogOwnersStore;
        private IBlogCommentsStore<BlogComment> _blogCommentsStore;
        private readonly IConfiguration _configuration;

        public PasswordHasher<ApplicationUser> _hasher { get; }
        public UserServiceImpl(IUserStore<ApplicationUser> userStore,
                               IUserRolesStore<IdentityRole, ApplicationUser> userRoleStore,
                               IConfiguration configuration,
                               IRoleStore<IdentityRole> roleStore,
                               IBlogOwnersStore<BlogOwner> blogOwnersStore,
                               IBlogCommentsStore<BlogComment> blogCommentsStore)
        {
            _userStore = userStore;
            _userRoleStore = userRoleStore;
            _roleStore = roleStore;
            _configuration = configuration;
            _hasher = new PasswordHasher<ApplicationUser>();
            _blogOwnersStore = blogOwnersStore;
            _blogCommentsStore = blogCommentsStore;
        }

        /*--------------------- Login -----------------------*/
        
        public async Task<ApplicationUser> FindByEmail(string email)
        {
            return await _userStore.FindByNameAsync(email, CancellationToken.None);
        }

        public async Task<ApplicationUser> FindById(string userId)
        {
            return await _userStore.FindByIdAsync(userId, CancellationToken.None);
        }

        public bool IsPasswordCorrect(ApplicationUser user, string password)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return (result== PasswordVerificationResult.Success)? true: false;
        }

        public async Task<RoleEnum> GetRole(ApplicationUser user)
        {
            IdentityRole userRole = await _userRoleStore.GetUserSingleRoleAsync(user.Id, CancellationToken.None);
            RoleEnum role;
            Enum.TryParse<RoleEnum>(userRole.Name, out role);
            return role;
        }

        /*--------------------- Registration -----------------------*/
        public async Task<ApplicationUser> CreateUser(RegisterUserDto dto)
        {
            IdentityResult result = await _userStore.CreateAsync(await RegisterUserDtoToApplicationUserEntity(dto), CancellationToken.None);
            if (!result.Succeeded)
                return null;
            return await FindByEmail(dto.Email);
        }

        public async Task<bool> AssignRole(ApplicationUser user, RoleEnum role)
        {
            int roleId = (int)role;
            IdentityRole userRole = await _roleStore.FindByIdAsync(roleId.ToString(), CancellationToken.None);
            if (userRole == null)
                return false;

            IdentityResult result = await _userRoleStore.AssignNewAsync(userRole, user, CancellationToken.None);
            if(result.Succeeded)
                return true;
            else
                return false;

        }

        /*--------------------- User CRUD -----------------------*/

        public async Task<UserInfoDto> GetByEmail(string email)
        {
            ApplicationUser user = await _userStore.FindByNameAsync(email, CancellationToken.None);
            return (user == null)? null : await ApplicationUserEntityToUserInfoDto(user,false);
        }
        public async Task<UserInfoDto> GetById(string userId)
        {
            ApplicationUser user = await _userStore.FindByIdAsync(userId, CancellationToken.None);
            return (user == null)? null : await ApplicationUserEntityToUserInfoDto(user,false);
        }

        public Task<UserInfoDto> UpdateUser(ApplicationUser detachedUser)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteUser(ApplicationUser user)
        {
            //Delete the user
            IdentityResult res =   await _userStore.DeleteAsync(user, CancellationToken.None);
            if (res.Succeeded)
                return true;
            else
                return false;
        }

        /*------------------ Mapper Methods ---------------------*/

        //  RegisterUserDto To ApplicationUser
        public async Task<ApplicationUser> RegisterUserDtoToApplicationUserEntity(RegisterUserDto dto)
        {
            var user = new ApplicationUser
            {
                Name = dto.Name,
                Email = dto.Email,
                UserName = dto.Email,
                Role = dto.Role
            };
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);
            await Task.Delay(0);
            return user;
        }

        //  ApplicationUser To UserInfoDto
        public async Task<UserInfoDto> ApplicationUserEntityToUserInfoDto(ApplicationUser entity, bool isTokenRequired)
        {
            UserInfoDto dto = new UserInfoDto { 
                Name= entity.Name,
                Email= entity.Email,
                Id= entity.Id,
                Role = entity.Role.ToString()
            };

            string token = string.Empty;
            if (isTokenRequired)
                token = await GenerateToken(entity);
            
            dto.Token = token;
            return dto;
        }

        /*----------------- Helper Methods -------------------*/

        private async Task<string> GenerateToken(ApplicationUser user)
        {
            //Generate the token

            //Fetch role of a user from DB
            IdentityRole userRole = await _userRoleStore.GetUserSingleRoleAsync(user.Id, CancellationToken.None);
            if (userRole == null)
                throw new UserCrudException("- Invalid userId OR\n-User has either no assigned roles OR\n- having multiple roles.");
            
            //Prepare list of claims
            List<Claim> myClaims = new List<Claim>()
                    {
                        new Claim("Id", user.Id.ToString()),
                        new Claim("Email", user.Email),
                        new Claim("Name", user.Name),
                        new Claim(ClaimTypes.Role, userRole.Name)
                    };

            //Generate security key
            string secret = _configuration.GetSection("Jwt:Secret").Value;
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
            SymmetricSecurityKey key = new SymmetricSecurityKey(secretBytes);

            //Generate credentials for token
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Create jwt token object
            JwtSecurityToken jwtToken = new JwtSecurityToken(
                signingCredentials: creds,
                claims: myClaims,
                expires: DateTime.Now.AddDays(1),
                issuer: _configuration.GetSection("AuthSettings:Issuer").Value,
                audience: _configuration.GetSection("AuthSettings:Audience").Value
                );

            //Generate string of token
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
}
