namespace BloggingApplication.Models.Dtos
{
    public class BlogAuthorsDto
    {
        public IEnumerable<UserInfoDto> Owners { get; set; }
        public IEnumerable<UserInfoDto> Editors { get; set; }
    }
}
