using System.ComponentModel.DataAnnotations;

namespace BloggingApplication.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public int Likes { get; set; }
    }
}
