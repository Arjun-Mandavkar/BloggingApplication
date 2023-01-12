using System.ComponentModel.DataAnnotations;

namespace BloggingApplication.Models.Dtos
{
    public class BlogOwnerDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int BlogId { get; set; }

    }
}
