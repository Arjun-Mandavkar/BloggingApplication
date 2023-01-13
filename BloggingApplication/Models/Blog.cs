using System.ComponentModel.DataAnnotations;

namespace BloggingApplication.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Likes { get; set; }
    }
}
