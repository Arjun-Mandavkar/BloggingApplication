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
            UserInfoDto user = await _userService.Login(dto);
            return Ok(user);
        }
    }
}
