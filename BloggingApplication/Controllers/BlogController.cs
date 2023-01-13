using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using BloggingApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Dapper.SqlMapper;

namespace BloggingApplication.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BlogController : ControllerBase
    {
        private IBlogService _blogService;
        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }
        /*--------------------- CRUD --------------------*/
        [HttpGet]
        public async Task<IEnumerable<Blog>> GetAll()
        {
            return await _blogService.GetAll();
        }

        [HttpGet("{blogId}")]
        public async Task<Blog> Get(int blogId)
        {
            return await _blogService.Get(blogId);
        }

        [HttpPost("Create")]
        public async Task<ApiResponseDto> Create(Blog blog)
        {
            IdentityResult result = _blogService.VerifyBlog(blog);
            if (!result.Succeeded)
                return ReturnError(result);

            result = await _blogService.Create(blog);
            if(! result.Succeeded)
                return ReturnError(result);

            return new ApiResponseDto(isSuccess: true, message: "Blog created successfully.");
        }

        [HttpPut("Update")]
        public async Task<ApiResponseDto> Update(Blog blog)
        {
            IdentityResult result = await _blogService.Update(blog);
            if (!result.Succeeded)
                return ReturnError(result);
            return new ApiResponseDto(isSuccess: true, message: "Blog updated successfully");

        }
        [HttpDelete("{blogId}")]
        public async Task<ApiResponseDto> Delete(int blogId)
        {
            IdentityResult result = await _blogService.Delete(blogId);
            if (!result.Succeeded)
                return ReturnError(result);
            return new ApiResponseDto(isSuccess: true, message: "Blog deleted successfully");
        }


        /*------------------------ Likes --------------------------*/
        [HttpGet("IsLiked/{blogId}")]
        public async Task<ActionResult<bool>> IsBlogLiked(int blogId)
        {
            return await _blogService.IsLiked(blogId);
        }

        [HttpGet("Like/{blogId}")]
        public async Task<ActionResult<ApiResponseDto>> LikeBlog(int blogId)
        {
            IdentityResult result = await _blogService.LikeBlog(blogId);
            if (!result.Succeeded)
                return ReturnError(result);
            return new ApiResponseDto(isSuccess: true, message: "Blog liked successfully");

        }
        [HttpDelete("Like/{blogId}")]
        public async Task<ActionResult<ApiResponseDto>> DeleteLikeBlog(int blogId)
        {
            IdentityResult result = await _blogService.DeleteLikeBlog(blogId);
            if (!result.Succeeded)
                return ReturnError(result);
            return new ApiResponseDto(isSuccess: true, message: "Blog like removed successfully");
        }

        /*----------------------- Comments ------------------------*/
        [HttpPost("Comment")]
        public async Task<ActionResult<ApiResponseDto>> CommentOnBlog(BlogCommentDto dto)
        {
            return await _blogService.CommentOnBlog(dto);
        }
        [HttpDelete("Comment")]
        public async Task<ActionResult<ApiResponseDto>> DeleteComment(BlogCommentDto dto)
        {
            return await _blogService.DeleteComment(dto);
        }
        [HttpPut("Comment")]
        public async Task<ActionResult<ApiResponseDto>> UpdateComment(BlogCommentDto dto)
        {
            return await _blogService.EditComment(dto);
        }
        [HttpGet("Comment/{blogId}")]
        public async Task<IEnumerable<BlogComment>> GetAllCommentsOfBlog(int blogId)
        {
            return await _blogService.GetAllCommentsOfBlog(blogId);
        }

        /*---------------------- Assign Role ----------------------*/
        [HttpPost("AssignRoles")]
        public async Task<ActionResult<ApiResponseDto>> AssignRoles(BlogRoleDto dto)
        {
            IdentityResult result = await _blogService.AssignRoles(dto);
            if (result.Succeeded)
                return new ApiResponseDto(isSuccess: true, message: "Roles assigned successfully.");
            else
                return ReturnError(result);
        }
        [HttpPost("RevokeRoles")]
        public async Task<ActionResult<ApiResponseDto>> RevokeRoles(BlogRoleDto dto)
        {
            IdentityResult result = await _blogService.RevokeRoles(dto);
            if (result.Succeeded)
                return new ApiResponseDto(isSuccess: true, message: "Roles revoked successfully.");
            else
                return ReturnError(result);
        }

        /*------------------- Helper methods -------------------*/
        private ApiResponseDto ReturnError(IdentityResult result)
        {
            if (result.Succeeded)
                return null;
            else
            {
                IdentityError error = result.Errors.FirstOrDefault(e => e.Code == "Message");
                return new ApiResponseDto(isSuccess: false, message: error.Description);
            }
        }
    }
}