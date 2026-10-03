namespace ATM.Core.Exceptions
{
    public class EmailVerificationException : Exception
    {
        public EmailVerificationException()
        {
        }

        public EmailVerificationException(string? message) : base(message)
        {
        }
    }
}
