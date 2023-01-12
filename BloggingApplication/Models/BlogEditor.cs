using System.ComponentModel.DataAnnotations;

namespace BloggingApplication.Models
{
    public class BlogEditor
    {
        [Required]
        public int BlogId { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
