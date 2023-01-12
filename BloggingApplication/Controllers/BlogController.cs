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
            return await _blogService.Create(blog);
        }
        [HttpPut("Update")]
        public async Task<ApiResponseDto> Update(Blog blog)
        {
            return await _blogService.Update(blog);
        }
        [HttpDelete("{blogId}")]
        public async Task<ApiResponseDto> Delete(int blogId)
        {
            return await _blogService.Delete(blogId);
        }


        /*------------------------ Likes --------------------------*/
        [HttpGet("Like/{blogId}")]
        public async Task<ActionResult<ApiResponseDto>> LikeBlog(int blogId)
        {
            return await _blogService.LikeBlog(blogId);
        }
        [HttpDelete("Like/{blogId}")]
        public async Task<ActionResult<ApiResponseDto>> DeleteLikeBlog(int blogId)
        {
            return await _blogService.DeleteLikeBlog(blogId);
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

        /*---------------------- Editor Role ----------------------*/
        [HttpPost("AssignEditor")]
        public async Task<ActionResult<ApiResponseDto>> AssignEditor(BlogEditorDto dto)
        {
            return await _blogService.AssignEditor(dto);
        }
        [HttpPost("RevokeEditor")]
        public async Task<ActionResult<ApiResponseDto>> RevokeEditor(BlogEditorDto dto)
        {
            return await _blogService.RevokeEditor(dto);
        }

        /*---------------------- Owner Role ----------------------*/
        [HttpPost("AssignOwner")]
        public async Task<ActionResult<ApiResponseDto>> AssignOwner(BlogOwnerDto dto)
        {
            return await _blogService.AssignOwner(dto);
        }
        [HttpPost("RevokeOwner")]
        public async Task<ActionResult<ApiResponseDto>> RevokeOwner(BlogOwnerDto dto)
        {
            return await _blogService.RevokeOwner(dto);
        }
    }
}
