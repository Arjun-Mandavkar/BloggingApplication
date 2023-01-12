namespace BloggingApplication.CustomExceptions
{
    public class BlogCrudException: Exception
    {
        public BlogCrudException(){ }
        public BlogCrudException(string message):base(message) { }
    }
}
