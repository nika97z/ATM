using ATM.Core.Models;

namespace ATM.Core.Interfaces
{
    public interface IEmailService
    {
        void SendVerificationCode(string toEmail, string name, string code);
        void SendLoanStatusNotification(ClientUser clientUser);
        void SendBanStatusNotification(ClientUser clientUser);
    }
}
