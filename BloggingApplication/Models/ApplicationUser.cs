using BloggingApplication.Models.Dtos;
using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string Name { get; set; }
        public RoleEnum Role { get; set; }
    }
}
