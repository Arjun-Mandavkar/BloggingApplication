using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using BloggingApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Transactions;

namespace BloggingApplication.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private PasswordHasher<ApplicationUser> _hasher { get; }
        private IUserService _userService { get; }

        public AuthController(IUserService userService)
        {
            _hasher = new PasswordHasher<ApplicationUser>();
            _userService = userService;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<UserInfoDto>> Register(RegisterUserDto dto)
        {
            ApplicationUser user = await _userService.FindByEmail(dto.Email);

            //Chech user exists or not
            if (user != null)
                return BadRequest("Email already taken. Try signing in.");

            using (var tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                //Create user
                user = await _userService.CreateUser(dto);
                if(user == null)
                    return BadRequest("User creation failed.");

                //Assign blogger role
                user.Role = RoleEnum.BLOGGER;
                bool res = await _userService.AssignRole(user, RoleEnum.BLOGGER);
                if (!res)
                    return StatusCode(500,"Assigning role to user failed.");
                tr.Complete();
            }
            return StatusCode(201, await _userService.ApplicationUserEntityToUserInfoDto(user, isTokenRequired: true));
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<UserInfoDto>> Login(LoginUserDto dto)
        {
            ApplicationUser user = await _userService.FindByEmail(dto.Email);

            //Chech user exists or not
            if (user == null)
                return BadRequest("Invalid Email. Try registring first.");

            //Verify password
            if (! _userService.IsPasswordCorrect(user, dto.Password))
                return BadRequest("Invalid Password.");

            //Fetch roles
            user.Role = await _userService.GetRole(user);
            if(user.Role == 0)
                return BadRequest("Roles not found for user.");

            //Prepare dto and return response
            return Ok(await _userService.ApplicationUserEntityToUserInfoDto(user, isTokenRequired: true));
        }
    }
}
