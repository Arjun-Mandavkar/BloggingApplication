using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using System.Security.Claims;

namespace BloggingApplication.Services
{
    public interface IBlogService
    {
        /*--------------------- CRUD -------------------------*/
        public Task<ApiResponseDto> Create(Blog blog);
        public Task<Blog> Get(int blogId);
        public Task<IEnumerable<Blog>> GetAll();
        public Task<ApiResponseDto> Delete(int blogId);
        public Task<ApiResponseDto> Update(Blog blog);

        /*--------------------- Like -------------------------*/
        public Task<ApiResponseDto> LikeBlog(int blogId);
        public Task<ApiResponseDto> DeleteLikeBlog(int blogId);

        /*--------------------- Comment -------------------------*/
        public Task<ApiResponseDto> CommentOnBlog(BlogCommentDto comment);
        public Task<ApiResponseDto> DeleteComment(BlogCommentDto comment);
        public Task<ApiResponseDto> EditComment(BlogCommentDto comment);
        public Task<IEnumerable<BlogComment>> GetAllCommentsOfBlog(int blogId);

        /*--------------------- Editor -------------------------*/
        public Task<ApiResponseDto> AssignEditor(BlogEditorDto dto);
        public Task<ApiResponseDto> RevokeEditor(BlogEditorDto dto);

        /*--------------------- Owner -------------------------*/
        public Task<ApiResponseDto> AssignOwner(BlogOwnerDto dto);
        public Task<ApiResponseDto> RevokeOwner(BlogOwnerDto dto);

        /*---------- Methods required for deleting User ---------*/
        public Task<bool> UpdateCommentForUserDeletion(ApplicationUser user);
        public Task<bool> UpdateOwnerEntryForUserDeletion(ApplicationUser user);
    }
}
