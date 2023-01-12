namespace BloggingApplication.CustomExceptions
{
    public class UserCrudException : Exception
    {
        public UserCrudException() { }
        public UserCrudException(string message) : base(message) { }
    }
}
