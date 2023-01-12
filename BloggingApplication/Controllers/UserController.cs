using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using BloggingApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggingApplication.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    public class UserController : ControllerBase
    {
        public IUserService _userService { get; }

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<UserInfoDto>> GetById(string userId)
        {
            UserInfoDto user = await _userService.GetById(userId);
            return Ok(user);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<UserInfoDto>> Create(RegisterUserDto dto)
        {
            UserInfoDto user = await _userService.Register(dto);
            return Ok(user);
        }

        [HttpDelete]
        [Route("{UserId}")]
        public async Task<ActionResult<UserInfoDto>> Delete(int UserId)
        {
            UserInfoDto user = await _userService.DeleteUser(UserId);
            return Ok(user);
        }
    }
}
