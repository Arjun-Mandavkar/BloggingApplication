using BloggingApplication.CustomExceptions;
using BloggingApplication.Models;
using BloggingApplication.Models.Dtos;
using BloggingApplication.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Transactions;

namespace BloggingApplication.Services.Implementations
{
    public class BlogServiceImpl : IBlogService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private IBlogStore<Blog> _blogStore;
        private IBlogLikesStore<Blog,ApplicationUser> _blogLikesStore;
        private IBlogCommentsStore<BlogComment> _blogCommentStore;
        private IBlogOwnersStore<BlogOwner> _blogOwnerStore;
        private IBlogEditorsStore<Blog, ApplicationUser> _blogEditorStore;
        private IUserStore<ApplicationUser> _userStore;
        private IUserRolesStore<IdentityRole,ApplicationUser> _userRolesStore;
        
        private readonly IConfiguration _configuration;
        public BlogServiceImpl(IHttpContextAccessor httpContext, 
                               IBlogStore<Blog> blogStore,
                               IConfiguration configuration,
                               IBlogLikesStore<Blog, ApplicationUser> blogLikesStore,
                               IBlogOwnersStore<BlogOwner> blogOwnerStore,
                               IBlogEditorsStore<Blog, ApplicationUser> blogEditorStore,
                               IUserStore<ApplicationUser> userStore,
                               IUserRolesStore<IdentityRole, ApplicationUser> userRolesStore,
                               IBlogCommentsStore<BlogComment> blogCommentStore)

        {
            _httpContextAccessor = httpContext;
            _blogStore = blogStore;
            _configuration = configuration;
            _blogLikesStore = blogLikesStore;
            _blogCommentStore = blogCommentStore;
            _blogOwnerStore = blogOwnerStore;
            _blogEditorStore = blogEditorStore;
            _userStore = userStore;
            _userRolesStore = userRolesStore;
        }

        /*--------------------- CRUD -------------------------*/
        public async Task<ApiResponseDto> Create(Blog blog)
        {
            //Check for character limits
            int titleLimit = Int32.Parse(_configuration.GetSection("Blog:TitleCharLimit").Value);
            int contentLimit = Int32.Parse(_configuration.GetSection("Blog:TitleCharLimit").Value);
            if (blog.Title.IsNullOrEmpty() || blog.Content.IsNullOrEmpty())
                throw new BlogCrudException($"Title or content cannot be empty.");
            if (blog.Title.Length > titleLimit)
                throw new BlogCrudException($"Title char limit is {titleLimit}.");
            if (blog.Content.Length > contentLimit)
                throw new BlogCrudException($"Content char limit is {contentLimit}.");

            using(var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                //Create blog from blog store
                Blog detachedBlog = await _blogStore.CreateAsync(blog);
                if (detachedBlog.Id == 0)
                    throw new BlogCrudException("Blog creation failed.");

                //Assign logged in user as owner of blog
                var result = await _blogOwnerStore.AssignOwner(await BlogToBlogOwner(blog));
                if (!result.Succeeded)
                    throw new BlogCrudException("Assigning you as an owner to the blog failed.");

                tx.Complete();
            }

            return new ApiResponseDto (isSuccess:true, message:"Blog created successfully.");
        }

        public async Task<Blog> Get(int blogId)
        {
            return await _blogStore.GetByIdAsync(blogId);
        }
        public Task<IEnumerable<Blog>> GetAll()
        {
            return _blogStore.GetAllAsync();
        }

        public async Task<ApiResponseDto> Update(Blog blog)
        {
            //Check for valid blogId
            Blog detachedBlog = await _blogStore.GetByIdAsync(blog.Id);
            if (detachedBlog == null)
                throw new InvalidOperationException("Invalid blog object.");

            //Fetch logged in user's details
            ApplicationUser user = await FetchLoggedInUser();

            //Check wheather user is one of the owners or editors or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));

            bool isEditor = await _blogEditorStore.IsEditor(blog, user);

            bool isAdmin = await IsUserAdmin();
            
            if(isAdmin || isOwner || isEditor)
            {
                IdentityResult result = await _blogStore.UpdateAsync(blog);
                if (!result.Succeeded)
                    throw new BlogCrudException("Blog updation failed.");
            }
            else
                throw new UnauthorizedUserException("You are not allowed to edit this blog.");
            
            return new ApiResponseDto(isSuccess:true, message:"Blog updated successfully.");
        }

        public async Task<ApiResponseDto> Delete(int blogId)
        {
            //Check for valid blogId
            Blog blog = await _blogStore.GetByIdAsync(blogId);
            if (blog == null)
                throw new BlogCrudException("Invalid blog Id OR blog already deleted.");

            //Check wheather user is one of the owners or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));
            bool isAdmin = await IsUserAdmin();

            //Delete blog from blog store
            if(isAdmin || isOwner)
            {
                IdentityResult result = await _blogStore.DeleteAsync(blog);
                if (!result.Succeeded)
                    throw new BlogCrudException("Blog deletion failed.");
            }
            else
                throw new UnauthorizedUserException("You are not allowed to delete this blog.");

            return new ApiResponseDto(isSuccess: true, message: "Blog deletd successfully.");
        }

        /*--------------------- Like -------------------------*/
        public async Task<ApiResponseDto> LikeBlog(int blogId)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(blogId);
            if (blog == null)
                throw new InvalidOperationException("Invalid blog Id.");

            //Fetch logged in user details
            ApplicationUser user = await FetchLoggedInUser();
            bool res = await _blogLikesStore.IsLikedAsync(blog, user);
            if (res)
                return new ApiResponseDto(false, "Already liked!");

            //Insert Like
            IdentityResult result = await _blogLikesStore.LikeAsync(blog, user);
            if (!result.Succeeded)
                throw new BlogOperationException("Blog like failed!");
            return new ApiResponseDto(true, "Successfully liked the blog.");
        }

        public async Task<ApiResponseDto> DeleteLikeBlog(int blogId)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(blogId);
            if (blog == null)
                throw new InvalidOperationException("Invalid blog Id.");

            //Fetch logged in user details
            ApplicationUser user = await FetchLoggedInUser();
            bool res = await _blogLikesStore.IsLikedAsync(blog, user);
            if (!res)
                return new ApiResponseDto(false, "You have not liked the blog.");

            //Remove Like
            IdentityResult result = await _blogLikesStore.UndoLikeAsync(blog, user);
            if (!result.Succeeded)
                throw new BlogOperationException("Remove like from blog failed!");
            return new ApiResponseDto(true, "Successfully unliked the blog.");
        }

        /*--------------------- Comment -------------------------*/
        public async Task<ApiResponseDto> CommentOnBlog(BlogCommentDto comment)
        {
            if(comment.Id != 0)
                return new ApiResponseDto(isSuccess: false, message: "Comment object should be transient [id should be null].");

            //Create entity of BlogComment
            BlogComment commentEntity = await BlogCommentDtoToEntity(comment);

            BlogComment detachedComment = await _blogCommentStore.CreateAsync(commentEntity);
            if (detachedComment == null) 
                throw new BlogOperationException("Comment insertion failed.");

            return new ApiResponseDto(isSuccess: true, message: "Comment inserted successfully.");
        }

        public async Task<ApiResponseDto> DeleteComment(BlogCommentDto comment)
        {
            BlogComment detachedObject = await _blogCommentStore.GetAsync(comment.Id);
            if (detachedObject == null)
                throw new BlogOperationException("Invalid comment Id.");

            ApplicationUser loggedInUser = await FetchLoggedInUser();

            Blog blog = await _blogStore.GetByIdAsync(detachedObject.BlogId);
            if (blog == null)
                throw new BlogOperationException("Invalid blog Id.");

            if (comment.Id == 0)
                return new ApiResponseDto(isSuccess: false, message: "Comment object should be detached.");

            //Check for correct user
            if (await IsUserAdmin())
            {
                //Allow admins to delete comment
            }
            else if (await _blogOwnerStore.IsOwner(BlogToBlogOwner(blog, loggedInUser)))
            {
                //Allow owners to delete comment
            }
            else if (loggedInUser.Id != detachedObject.UserId)
                throw new BlogOperationException("Not authorized to delete the comment.");

            //Check for correct blog id
            if (detachedObject.BlogId != comment.BlogId)
                throw new BlogOperationException("Invalid combination of blog and comment.");

            IdentityResult result = await _blogCommentStore.DeleteAsync(await BlogCommentDtoToEntity(comment));
            if (!result.Succeeded)
                throw new BlogOperationException("Comment deletion failed.");

            return new ApiResponseDto(isSuccess: true, message: "Comment deleted successfully.");
        }
        public async Task<ApiResponseDto> EditComment(BlogCommentDto comment)
        {
            BlogComment detachedObject = await _blogCommentStore.GetAsync(comment.Id);
            ApplicationUser loggedInUser = await FetchLoggedInUser();

            //Check for correct user
            if (loggedInUser.Id != detachedObject.UserId)
                throw new UnauthorizedUserException("Not authorized to edit the comment.");

            if (comment.Id == 0)
                return new ApiResponseDto(isSuccess: false, message: "Comment object should be detached.");
            
            //Check for correct blog id
            if (detachedObject.BlogId != comment.BlogId)
                throw new BlogOperationException("Invalid combination of blog and comment.");

            IdentityResult result = await _blogCommentStore.UpdateAsync(await BlogCommentDtoToEntity(comment));
            if (!result.Succeeded)
                throw new BlogOperationException("Comment updation failed.");

            return new ApiResponseDto(isSuccess: true, message: "Comment updated successfully.");
        }
        public async Task<IEnumerable<BlogComment>> GetAllCommentsOfBlog(int blogId)
        {
            return await _blogCommentStore.GetAllFromBlogAsync(blogId);
        }

        /*--------------------- Editor -------------------------*/
        public async Task<ApiResponseDto> AssignEditor(BlogEditorDto dto)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(dto.BlogId);
            if (blog == null)
                throw new BlogOperationException("Invalid blog Id");

            //Check logged in user is either owner or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));
            bool isAdmin = await IsUserAdmin();

            if (isAdmin || isOwner)
            {
                //Fetch the specified user object
                ApplicationUser user = await _userStore.FindByIdAsync(dto.UserId.ToString(),CancellationToken.None);
                if (user == null)
                    throw new BlogOperationException("Provided user id is incorrect.");

                bool isSpecifiedUserEditor = await _blogEditorStore.IsEditor(blog, user);

                if (!isSpecifiedUserEditor)
                {
                    //Insert entry into editor table
                    IdentityResult result = await _blogEditorStore.AssignEditor(blog, user);
                    if (!result.Succeeded)
                        throw new BlogOperationException("An error occured while assigning editor role.");
                }
                else
                    throw new BlogOperationException("User already an editor.");
                
                return new ApiResponseDto(isSuccess: true, message: "Editor assigned successfully.");
            }else
                throw new UnauthorizedUserException("Only blog owners can add editors to blog.");
        }

        public async Task<ApiResponseDto> RevokeEditor(BlogEditorDto dto)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(dto.BlogId);
            if (blog == null)
                throw new BlogOperationException("Invalid blog Id");

            //Check logged in user is either owner or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));
            bool isAdmin = await IsUserAdmin();

            if (isAdmin || isOwner)
            {
                //Fetch the specified user object
                ApplicationUser user = await _userStore.FindByIdAsync(dto.UserId.ToString(), CancellationToken.None);
                if (user == null)
                    throw new BlogOperationException("Provided user id is incorrect.");

                bool isSpecifiedUserEditor = await _blogEditorStore.IsEditor(blog, user);
                if (isSpecifiedUserEditor)
                {
                    //Remove entry into editor table
                    IdentityResult result = await _blogEditorStore.RevokeEditor(blog, user);
                    if (!result.Succeeded)
                        throw new BlogOperationException("An error occured while removing editor role.");
                }
                else
                    throw new BlogOperationException("User does not have an editor role.");

                return new ApiResponseDto(isSuccess: true, message: "Editor removed successfully.");
            }
            else
                throw new UnauthorizedUserException("Only blog owners can add editors to blog.");
        }

        /*--------------------- Owner -------------------------*/
        public async Task<ApiResponseDto> AssignOwner(BlogOwnerDto dto)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(dto.BlogId);
            if (blog == null)
                throw new BlogOperationException("Invalid blog Id");

            //Check logged in user is either owner or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));
            bool isAdmin = await IsUserAdmin();

            if (isAdmin || isOwner)
            {
                //Fetch the specified user object
                ApplicationUser user = await _userStore.FindByIdAsync(dto.UserId.ToString(), CancellationToken.None);
                if (user == null)
                    throw new BlogOperationException("Provided user id is incorrect.");

                bool isSpecifiedUserOwner = await _blogOwnerStore.IsOwner(BlogToBlogOwner(blog,user));
                if (!isSpecifiedUserOwner)
                {
                    //Insert entry in owner table
                    IdentityResult result = await _blogOwnerStore.AssignOwner(BlogToBlogOwner(blog,user));
                    if (!result.Succeeded)
                        throw new BlogOperationException("Assigning user as owner failed.");

                    return new ApiResponseDto(isSuccess: true, message: "User assigned owner role successfully");
                }
                else
                    throw new BlogOperationException("User already have an owner role.");
            }
            else
                throw new UnauthorizedUserException("Only blog owners can add owners to blog.");
        }

        public async Task<ApiResponseDto> RevokeOwner(BlogOwnerDto dto)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(dto.BlogId);
            if (blog == null)
                throw new BlogOperationException("Invalid blog Id");

            //Check logged in user is either owner or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));
            bool isAdmin = await IsUserAdmin();

            if (isAdmin || isOwner)
            {
                //Fetch the specified user object
                ApplicationUser user = await _userStore.FindByIdAsync(dto.UserId.ToString(), CancellationToken.None);
                if (user == null)
                    throw new BlogOperationException("Provided user id is incorrect.");

                bool isSpecifiedUserOwner = await _blogOwnerStore.IsOwner(BlogToBlogOwner(blog, user));
                if (isSpecifiedUserOwner)
                {
                    //Remove entry from owner table
                    IdentityResult result = await _blogOwnerStore.RevokeOwner(BlogToBlogOwner(blog, user));
                    if (!result.Succeeded)
                        throw new BlogOperationException("Removing user from owner role failed.");

                    return new ApiResponseDto(isSuccess: true, message: "User assigned owner role successfully");
                }
                else
                    throw new BlogOperationException("User does not have owner role.");
            }
            else
                throw new UnauthorizedUserException("Only blog owners can remove owners from blog.");
        }


        /*--------------------- Dto Mapping methods ----------------*/
        private async Task<BlogOwner> BlogToBlogOwner(Blog blog)
        {
            ApplicationUser user = await FetchLoggedInUser();
            return new BlogOwner
            {
                BlogId = blog.Id,
                UserId = user.Id,
                OwnerName = user.Name,
                IsOwnerExists = true
            };
        }
        private BlogOwner BlogToBlogOwner(Blog blog, ApplicationUser user)
        {
            return new BlogOwner
            {
                BlogId = blog.Id,
                UserId = user.Id,
                OwnerName = user.Name,
                IsOwnerExists = true
            };
        }

        private async Task<BlogComment> BlogCommentDtoToEntity(BlogCommentDto dto)
        {
            Blog blog = await _blogStore.GetByIdAsync(dto.BlogId);
            if (blog == null)
                throw new BlogOperationException("Specified blog does not exists.");

            ApplicationUser user = await FetchLoggedInUser();

            if (dto.Id == 0)
            {
                return new BlogComment
                {
                    Id = 0,
                    BlogId = dto.BlogId,
                    UserId= user.Id,
                    Text= dto.Text,
                    UserName= user.Name,
                    IsUserExists = true
                };
            }
            else
            {
                BlogComment newComment = await _blogCommentStore.GetAsync(dto.Id);
                newComment.Text = dto.Text;
                return newComment;
            }
        }

        /*--------------------- Helper Methods ---------------------*/
        private async Task<bool> IsUserAdmin()
        {
            ApplicationUser user = await FetchLoggedInUser();

            bool isAdmin = false;
            IdentityRole role = await _userRolesStore.GetUserSingleRoleAsync(user, CancellationToken.None);
            if (role != null)
                isAdmin = role.Name.Equals("ADMIN");

            return isAdmin;
        }

        private async Task<ApplicationUser> FetchLoggedInUser()
        {
            string userId = _httpContextAccessor.HttpContext.User.Claims
                            .FirstOrDefault(c => c.Type == "Id").Value;

            ApplicationUser user = await _userStore.FindByIdAsync(userId, CancellationToken.None);
            if (user == null)
                throw new BlogCrudException("Logged in user details not found.");

            return user;
        }

    }
}
