using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using BloggingApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Transactions;

namespace BloggingApplication.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    public class UserController : ControllerBase
    {
        private IUserService _userService { get; }
        private IBlogService _blogService { get; }
        
        public UserController(IUserService userService, IBlogService blogService)
        {
            _userService = userService;
            _blogService = blogService;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<UserInfoDto>> GetById(string userId)
        {
            UserInfoDto user = await _userService.GetById(userId);
            return Ok(user);
        }

        [HttpGet]
        [Route("{email}")]
        public async Task<ActionResult<UserInfoDto>> GetByEmail(string email)
        {
            UserInfoDto user = await _userService.GetByEmail(email);
            return Ok(user);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<UserInfoDto>> Create(RegisterUserDto dto)
        {
            ApplicationUser user = await _userService.FindByEmail(dto.Email);

            //Chech user exists or not
            if (user != null)
                return BadRequest("Email already taken.");

            using (var tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                //Create user
                user = await _userService.CreateUser(dto);
                if (user == null)
                    return BadRequest("User creation failed.");

                //Assign role from dto
                user.Role = dto.Role;
                bool res = await _userService.AssignRole(user, dto.Role);
                if (!res)
                    return StatusCode(500, "Assigning role to user failed.");
                tr.Complete();
            }
            return StatusCode(201, await _userService.ApplicationUserEntityToUserInfoDto(user, isTokenRequired: false)); ;
        }

        [HttpDelete]
        [Route("{userId}")]
        public async Task<ActionResult<UserInfoDto>> Delete(int userId)
        {
            ApplicationUser user = await _userService.FindById(userId.ToString());

            //Chech user exists or not
            if (user == null)
                return BadRequest("User not found.");

            //Check for role and assign to entity object
            user.Role = await _userService.GetRole(user);

            using (var tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                //Delete the user
                bool res = await _userService.DeleteUser(user);
                if (!res)
                    return BadRequest("Invalid user id.");

                //set isOwnerExists false in owners table
                res = await _blogService.UpdateOwnerEntryForUserDeletion(user);
                if (!res)
                    return StatusCode(500, "Failed to update owner table.");

                //Set isUserExists false in comments table
                res = await _blogService.UpdateCommentForUserDeletion(user);
                if (!res)
                    return StatusCode(500, "Failed to update comment table.");

                tr.Complete();
            }

            return Ok(_userService.ApplicationUserEntityToUserInfoDto(user, isTokenRequired:false));
        }
    }
}
