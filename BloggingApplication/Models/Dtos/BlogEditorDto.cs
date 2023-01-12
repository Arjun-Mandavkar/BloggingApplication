using System.ComponentModel.DataAnnotations;

namespace BloggingApplication.Models.Dtos
{
    public class BlogEditorDto
    {
        [Required]
        public int BlogId { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
