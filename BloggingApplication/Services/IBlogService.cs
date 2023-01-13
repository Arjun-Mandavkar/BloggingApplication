using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BloggingApplication.Services
{
    public interface IBlogService
    {
        /*--------------------- CRUD -------------------------*/
        public Task<IdentityResult> Create(Blog blog);
        public IdentityResult VerifyBlog(Blog blog);
        public Task<Blog> Get(int blogId);
        public Task<IEnumerable<Blog>> GetAll();
        public Task<IdentityResult> Delete(int blogId);
        public Task<IdentityResult> Update(Blog blog);

        /*--------------------- Like -------------------------*/
        public Task<bool> IsLiked(int blogId);
        public Task<IdentityResult> LikeBlog(int blogId);
        public Task<IdentityResult> DeleteLikeBlog(int blogId);

        /*--------------------- Comment -------------------------*/
        public Task<ApiResponseDto> CommentOnBlog(BlogCommentDto comment);
        public Task<ApiResponseDto> DeleteComment(BlogCommentDto comment);
        public Task<ApiResponseDto> EditComment(BlogCommentDto comment);
        public Task<IEnumerable<BlogComment>> GetAllCommentsOfBlog(int blogId);

        /*------------------- Assign Roles -------------------*/
        public Task<IdentityResult> AssignRoles(BlogRoleDto dto);
        public Task<IdentityResult> RevokeRoles(BlogRoleDto dto);

        /*--------------------- Editor -------------------------*/
        public Task<IdentityResult> AssignEditor(Blog blog, ApplicationUser user);
        public Task<IdentityResult> RevokeEditor(Blog blog, ApplicationUser user);

        /*--------------------- Owner -------------------------*/
        public Task<IdentityResult> AssignOwner(Blog blog, ApplicationUser user);
        public Task<IdentityResult> RevokeOwner(Blog blog, ApplicationUser user);

        /*---------- Methods required for deleting User ---------*/
        public Task<bool> UpdateCommentForUserDeletion(ApplicationUser user);
        public Task<bool> UpdateOwnerEntryForUserDeletion(ApplicationUser user);
    }
}
