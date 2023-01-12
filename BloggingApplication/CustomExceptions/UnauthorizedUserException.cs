namespace BloggingApplication.CustomExceptions
{
    public class UnauthorizedUserException : Exception
    {
        public UnauthorizedUserException(){ }

        public UnauthorizedUserException(string message):base(message) { }
    }
}
