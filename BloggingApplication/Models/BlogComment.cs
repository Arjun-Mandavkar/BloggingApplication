using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BloggingApplication.Models
{
    public class BlogComment : ResponseModel
    {
        [Key]        
        public int Id { get; set; }
        [Required]
        public int BlogId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Text { get; set; } = string.Empty;
        [Required]
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public bool IsUserExists { get; set; } = true;
    }
}
