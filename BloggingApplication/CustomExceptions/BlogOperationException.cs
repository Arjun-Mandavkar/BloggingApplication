namespace BloggingApplication.CustomExceptions
{
    public class BlogOperationException:Exception
    {
        public BlogOperationException()
        {

        }
        public BlogOperationException(string message):base(message) { }
    }
}
