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
        private IBlogLikesStore<Blog, ApplicationUser> _blogLikesStore;
        private IBlogCommentsStore<BlogComment> _blogCommentStore;
        private IBlogOwnersStore<BlogOwner> _blogOwnerStore;
        private IBlogEditorsStore<Blog, ApplicationUser> _blogEditorStore;
        private IUserStore<ApplicationUser> _userStore;
        private IUserRolesStore<IdentityRole, ApplicationUser> _userRolesStore;
        private Dictionary<string, string> Messages = new Dictionary<string, string>
        {
            {"message1","User already have an editor role." },
            {"message2","User already have an owner role." },
            {"message3","User does not have an editor role." },
            {"message4","User does not have an owner role." }
        };

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
        public async Task<IdentityResult> Create(Blog blog)
        {
            using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
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

            return IdentityResult.Success;
        }
        public IdentityResult VerifyBlog(Blog blog)
        {
            //Check for character limits
            int titleLimit = Int32.Parse(_configuration.GetSection("Blog:TitleCharLimit").Value);
            int contentLimit = Int32.Parse(_configuration.GetSection("Blog:ContentCharLimit").Value);

            if (blog.Title.IsNullOrEmpty() || blog.Content.IsNullOrEmpty())
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Title or content not allowed to be empty." });
            if (blog.Title.Length > titleLimit)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = $"Title char limit is {titleLimit}." });
            if (blog.Content.Length > contentLimit)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = $"Content char limit is {contentLimit}." });

            return IdentityResult.Success;
        }
        public async Task<Blog> Get(int blogId)
        {
            return await _blogStore.GetByIdAsync(blogId);
        }
        public Task<IEnumerable<Blog>> GetAll()
        {
            return _blogStore.GetAllAsync();
        }

        public async Task<IdentityResult> Update(Blog blog)
        {
            //Check for valid blogId
            Blog detachedBlog = await Get(blog.Id);
            if (detachedBlog == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Invalid blog id." });

            //Fetch logged in user's details
            ApplicationUser user = await FetchLoggedInUser();

            //Check wheather user is one of the owners or editors or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));

            bool isEditor = await _blogEditorStore.IsEditor(blog, user);

            bool isAdmin = await IsUserAdmin();

            if (isAdmin || isOwner || isEditor)
            {
                IdentityResult result = await _blogStore.UpdateAsync(blog);
                if (!result.Succeeded)
                    return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Blog updation failed." });
            }
            else
                throw new UnauthorizedUserException("You are not allowed to edit this blog.");

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> Delete(int blogId)
        {
            //Check for valid blogId
            Blog blog = await _blogStore.GetByIdAsync(blogId);
            if (blog == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Invalid blog Id OR blog already deleted." });

            //Check wheather user is one of the owners or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));
            bool isAdmin = await IsUserAdmin();

            //Delete blog from blog store
            if (isAdmin || isOwner)
            {
                IdentityResult result = await _blogStore.DeleteAsync(blog.Id);
                if (!result.Succeeded)
                    return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Blog deletion failed." });
            }
            else
                throw new UnauthorizedUserException("You are not allowed to delete this blog.");

            return IdentityResult.Success;
        }

        /*--------------------- Like -------------------------*/

        public async Task<bool> IsLiked(int blogId)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(blogId);
            if (blog == null) return false;

            //Fetch logged in user details
            ApplicationUser user = await FetchLoggedInUser();

            return await _blogLikesStore.IsLikedAsync(blog, user);
        }
        public async Task<IdentityResult> LikeBlog(int blogId)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(blogId);
            if (blog == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Blog not found" }); ;

            //Fetch logged in user details
            ApplicationUser user = await FetchLoggedInUser();

            bool res = await _blogLikesStore.IsLikedAsync(blog, user);
            if (res)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Already Liked" });
            else
            {
                using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    //Insert Like
                    IdentityResult result = await _blogLikesStore.LikeAsync(blog, user);
                    if (!result.Succeeded)
                        return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Blog like failed!" });

                    await _blogStore.IncrementLike(blog.Id);
                    if (!result.Succeeded)
                        return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Blog like failed!" });
                    tx.Complete();
                }
                return IdentityResult.Success;
            }
        }

        public async Task<IdentityResult> DeleteLikeBlog(int blogId)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(blogId);
            if (blog == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Blog not found" }); ;

            //Fetch logged in user details
            ApplicationUser user = await FetchLoggedInUser();

            bool res = await _blogLikesStore.IsLikedAsync(blog, user);
            if (!res)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Like not found" });
            else
            {
                using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    //Remove Like
                    IdentityResult result = await _blogLikesStore.UndoLikeAsync(blog, user);
                    if (!result.Succeeded)
                        return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Remove like from blog failed!" });
                    
                    result = await _blogStore.DecrementLike(blog.Id);
                    if (!result.Succeeded)
                        return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Remove like from blog failed!" });
                    
                    tx.Complete();
                }
                return IdentityResult.Success;
            }
        }

        /*--------------------- Comment -------------------------*/
        public async Task<IdentityResult> CommentOnBlog(BlogCommentDto comment)
        {
            if (comment.Id != 0)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Comment object should be transient [id should be null]." });

            //Create entity of BlogComment
            BlogComment commentEntity = await BlogCommentDtoToEntity(comment);

            BlogComment detachedComment = await _blogCommentStore.CreateAsync(commentEntity);
            if (detachedComment == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Comment insertion failed." });

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteComment(BlogCommentDto comment)
        {
            BlogComment detachedObject = await _blogCommentStore.GetAsync(comment.Id);
            if (detachedObject == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Invalid comment Id." });

            ApplicationUser loggedInUser = await FetchLoggedInUser();

            Blog blog = await _blogStore.GetByIdAsync(detachedObject.BlogId);
            if (blog == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Invalid blog Id." });

            if (comment.Id == 0)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Comment object should be detached." });

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
                throw new UnauthorizedUserException("Not authorized to delete the comment.");

            //Check for correct blog id
            if (detachedObject.BlogId != comment.BlogId)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Invalid combination of blog and comment." });

            IdentityResult result = await _blogCommentStore.DeleteAsync(await BlogCommentDtoToEntity(comment));
            if (!result.Succeeded)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Comment deletion failed." });

            return IdentityResult.Success;
        }
        public async Task<IdentityResult> EditComment(BlogCommentDto comment)
        {
            BlogComment detachedObject = await _blogCommentStore.GetAsync(comment.Id);
            ApplicationUser loggedInUser = await FetchLoggedInUser();

            //Check for correct user
            if (loggedInUser.Id != detachedObject.UserId)
                throw new UnauthorizedUserException("Not authorized to edit the comment.");

            if (comment.Id == 0)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Comment object should be detached." });

            //Check for correct blog id
            if (detachedObject.BlogId != comment.BlogId)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Invalid combination of blog and comment." }); 

            IdentityResult result = await _blogCommentStore.UpdateAsync(await BlogCommentDtoToEntity(comment));
            if (!result.Succeeded)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Comment updation failed." });

            return IdentityResult.Success;
        }
        public async Task<IEnumerable<BlogComment>> GetAllCommentsOfBlog(int blogId)
        {
            return await _blogCommentStore.GetAllFromBlogAsync(blogId);
        }


        /*------------------- Assign Roles -------------------*/
        public async Task<IdentityResult> AssignRoles(BlogRoleDto dto)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(dto.BlogId);
            if (blog == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Invalid blog id." });

            //Check logged in user is either owner or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));
            bool isAdmin = await IsUserAdmin();

            if (isOwner || isAdmin)
            {
                //Fetch the specified user object
                ApplicationUser user = await _userStore.FindByIdAsync(dto.UserId.ToString(), CancellationToken.None);
                if (user == null)
                    return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "User not found." });
                IdentityResult result;

                //Assign roles one by one
                if (dto.Roles.Contains(BlogRoleEnum.EDITOR))
                {
                    result = await AssignEditor(blog, user);

                    //If failed because of already assigned then skip to next step
                    if (result.Errors.Any())
                    {
                        IdentityError error = result.Errors.FirstOrDefault(e => e.Code == "Message");
                        if (error.Description != Messages["message1"])
                            return result;
                    }
                }
                if (dto.Roles.Contains(BlogRoleEnum.OWNER))
                {
                    result = await AssignOwner(blog, user);
                    if (result.Errors.Any())
                    {
                        IdentityError error = result.Errors.FirstOrDefault(e => e.Code == "Message");
                        if (error.Description != Messages["message2"])
                            return result;
                    }
                }
            }
            else
                throw new UnauthorizedUserException("Not authorized to assign roles");

            return IdentityResult.Success;
        }
        public async Task<IdentityResult> RevokeRoles(BlogRoleDto dto)
        {
            //Fetch detached blog object
            Blog blog = await _blogStore.GetByIdAsync(dto.BlogId);
            if (blog == null)
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Invalid blog id." });

            //Check logged in user is either owner or admin
            bool isOwner = await _blogOwnerStore.IsOwner(await BlogToBlogOwner(blog));
            bool isAdmin = await IsUserAdmin();
            if (isOwner || isAdmin)
            {
                //Fetch the specified user object
                ApplicationUser user = await _userStore.FindByIdAsync(dto.UserId.ToString(), CancellationToken.None);
                if (user == null)
                    return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "User not found." });

                IdentityResult result;

                //Revoke roles one by one
                if (dto.Roles.Contains(BlogRoleEnum.EDITOR))
                {
                    result = await RevokeEditor(blog, user);
                    //If failed because of not having role then skip to next step
                    if (result.Errors.Any())
                    {
                        IdentityError error = result.Errors.FirstOrDefault(e => e.Code == "Message");
                        if (error.Description != Messages["message3"])
                            return result;
                    }
                }
                if (dto.Roles.Contains(BlogRoleEnum.OWNER))
                {
                    result = await RevokeOwner(blog, user);
                    //If failed because of not having role then skip to next step
                    if (result.Errors.Any())
                    {
                        IdentityError error = result.Errors.FirstOrDefault(e => e.Code == "Message");
                        if (error.Description != Messages["message3"])
                            return result;
                    }
                }
            }
            else
                throw new UnauthorizedUserException("Not authorized to assign roles");
            return IdentityResult.Success;
        }

        /*--------------------- Editor -------------------------*/
        public async Task<IdentityResult> AssignEditor(Blog blog, ApplicationUser user)
        {
            bool isSpecifiedUserEditor = await _blogEditorStore.IsEditor(blog, user);

            if (!isSpecifiedUserEditor)
            {
                //Insert entry into editor table
                IdentityResult result = await _blogEditorStore.AssignEditor(blog, user);
                if (!result.Succeeded)
                    return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "An error occured while assigning editor role." });
                return IdentityResult.Success;
            }
            else
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = Messages["message1"] });
        }

        public async Task<IdentityResult> RevokeEditor(Blog blog, ApplicationUser user)
        {
            bool isSpecifiedUserEditor = await _blogEditorStore.IsEditor(blog, user);

            if (isSpecifiedUserEditor)
            {
                //Remove entry into editor table
                IdentityResult result = await _blogEditorStore.RevokeEditor(blog, user);
                if (!result.Succeeded)
                    return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "An error occured while revoking editor role." });
                return IdentityResult.Success;
            }
            else
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = Messages["message3"] });
        }

        /*--------------------- Owner -------------------------*/
        public async Task<IdentityResult> AssignOwner(Blog blog, ApplicationUser user)
        {
            bool isSpecifiedUserOwner = await _blogOwnerStore.IsOwner(BlogToBlogOwner(blog, user));
            if (!isSpecifiedUserOwner)
            {
                //Insert entry in owner table
                IdentityResult result = await _blogOwnerStore.AssignOwner(BlogToBlogOwner(blog, user));
                if (!result.Succeeded)
                    IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Assigning user as owner failed." });

                return IdentityResult.Success;
            }
            else
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = Messages["message2"] });
        }

        public async Task<IdentityResult> RevokeOwner(Blog blog, ApplicationUser user)
        {
            bool isSpecifiedUserOwner = await _blogOwnerStore.IsOwner(BlogToBlogOwner(blog, user));
            if (isSpecifiedUserOwner)
            {
                //Remove entry from owner table
                IdentityResult result = await _blogOwnerStore.RevokeOwner(BlogToBlogOwner(blog, user));
                if (!result.Succeeded)
                    return IdentityResult.Failed(new IdentityError { Code = "Message", Description = "Removing user from owner role failed." });

                return IdentityResult.Success;
            }
            else
                return IdentityResult.Failed(new IdentityError { Code = "Message", Description = Messages["message4"] });
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
                    UserId = user.Id,
                    Text = dto.Text,
                    UserName = user.Name,
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
            IdentityRole role = await _userRolesStore.GetUserSingleRoleAsync(user.Id, CancellationToken.None);
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

        public async Task<bool> UpdateCommentForUserDeletion(ApplicationUser user)
        {
            IdentityResult result = await _blogCommentStore.SetIsUserExistsFalse(user.Id);
            if (result.Succeeded)
                return true;
            else
                return false;
        }

        public async Task<bool> UpdateOwnerEntryForUserDeletion(ApplicationUser user)
        {
            IdentityResult result = await _blogOwnerStore.SetIsOwnerExistsFalse(user.Id);
            if (result.Succeeded)
                return true;
            else
                return false;
        }
    }
}
