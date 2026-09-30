namespace ATM.Core.Exceptions
{
    public class DeleteAccountExtension : Exception
    {
        public DeleteAccountExtension() : base("Cannot delete user with balance.")
        {
        }

        public DeleteAccountExtension(string? message) : base(message)
        {
        }
    }
}
