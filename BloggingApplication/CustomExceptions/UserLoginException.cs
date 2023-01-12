namespace BloggingApplication.CustomExceptions
{
    public class UserLoginException : Exception
    {
        public UserLoginException() { }
        public UserLoginException(string message) : base(message) { }
    }
}
