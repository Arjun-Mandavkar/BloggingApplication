using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using BloggingApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            UserInfoDto user = await _userService.Register(dto);
            return Ok(user);
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
