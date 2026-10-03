namespace ATM.Core.Exceptions
{
    public class UserBannedException : Exception
    {
        public UserBannedException()
        {
        }

        public UserBannedException(string? message) : base(message)
        {
        }
    }
}
