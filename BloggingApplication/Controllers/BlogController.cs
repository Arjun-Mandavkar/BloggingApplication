using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using BloggingApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using static Dapper.SqlMapper;

namespace BloggingApplication.Controllers
{
    public class BlogResponse
    {
        public IEnumerable<ResponseModel> Response { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }

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
            IdentityResult result = await _blogService.CommentOnBlog(dto);
            if (!result.Succeeded)
                return ReturnError(result);
            return new ApiResponseDto(isSuccess: true, message: "Comment inserted successfully");
        }

        [HttpDelete("Comment/Delete")]
        public async Task<ActionResult<ApiResponseDto>> DeleteComment(BlogCommentDto dto)
        {
            IdentityResult result = await _blogService.DeleteComment(dto);
            if (!result.Succeeded)
                return ReturnError(result);
            return new ApiResponseDto(isSuccess: true, message: "Comment deleted successfully");
        }

        [HttpPut("Comment")]
        public async Task<ActionResult<ApiResponseDto>> UpdateComment(BlogCommentDto dto)
        {
            IdentityResult result = await _blogService.EditComment(dto);
            if (!result.Succeeded)
                return ReturnError(result);
            return new ApiResponseDto(isSuccess: true, message: "Comment updated successfully");
        }

        [HttpGet("Comment/{blogId}")]
        public async Task<BlogResponse> GetAllCommentsOfBlog(int blogId)
        {
            BlogResponse response = new BlogResponse();
            try
            {
                var comments = await _blogService.GetAllCommentsOfBlog(blogId);
                response.Response = comments;
                response.StatusCode = HttpStatusCode.OK;
            }
            catch(Exception ex)
            {
                return response;
            }

            return response;
        }

        /*---------------------- Assign Role ----------------------*/
        [HttpGet("Authors/{blogId}")]
        public async Task<ActionResult<BlogAuthorsDto>> GetAuthors(int blogId)
        {
            return await _blogService.GetAuthors(blogId);
        }

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