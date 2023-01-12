namespace BloggingApplication.CustomExceptions
{
    public class UserRegistrationException : Exception
    {
        public UserRegistrationException() { }
        public UserRegistrationException(string message) : base(message) { }
    }
}
