using Microsoft.AspNetCore.Identity;

namespace BloggingApplication.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
