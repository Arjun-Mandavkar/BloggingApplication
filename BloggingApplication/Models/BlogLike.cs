using System.ComponentModel.DataAnnotations;

namespace BloggingApplication.Models
{
    public class BlogLike
    {
        [Required]
        public int BlogId { get; set; }
        [Required]
        public int UserId { get; set; }

    }
}
