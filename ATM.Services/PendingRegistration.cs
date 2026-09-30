using ATM.Core.Models;

namespace ATM.Services
{
    public class PendingRegistration
    {
        internal PendingRegistration(ClientUser clientUser, string code, DateTime expiresAt, int attemptsLeft)
        {
            ClientUser = clientUser;
            Code = code;
            ExpiresAt = expiresAt;
            AttemptsLeft = attemptsLeft;
        }

        public ClientUser ClientUser { get; }
        public int AttemptsLeft { get; internal set; }
        internal string Code { get; }
        internal DateTime ExpiresAt { get; }
    }
}
