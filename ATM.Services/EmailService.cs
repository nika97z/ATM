using ATM.Core.Enums;
using ATM.Core.Interfaces;
using ATM.Core.Models;
using System.Net;
using System.Net.Mail;

namespace ATM.Services
{
    public class EmailService : IEmailService
    {
        private const string SenderAddress = "ziraqishvili3@gmail.com";
        private const string AppPassword = "lddjxfkjijjdsjby";

        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderName = "ATM Bank";

        public void SendVerificationCode(string toEmail, string name, string code)
        {
            string body =
                $"Hello {name},\n\n" +
                $"Your ATM verification code is: {code}\n\n" +
                "If you did not try to register, you can ignore this email.";
            SendEmail(toEmail, "Your ATM verification code", body);
        }

        public void SendLoanStatusNotification(ClientUser clientUser)
        {
            var loan = clientUser.Loan;
            string subject;
            string body;
            if (loan.Status == LoanStatus.Approved)
            {
                subject = "Your loan has been approved";
                body =
                    $"Hello {clientUser.name},\n\n" +
                    $"Good news! Your loan request of {loan.RequestedAmount} gel for {loan.Time} months has been approved.\n" +
                    $"The money has been deposited to your account {loan.Account}.";
            }
            else if (loan.Status == LoanStatus.Rejected)
            {
                subject = "Your loan request has been rejected";
                body =
                    $"Hello {clientUser.name},\n\n" +
                    $"Unfortunately, your loan request of {loan.RequestedAmount} gel for {loan.Time} months has been rejected.";
            }
            else
            {
                throw new InvalidOperationException($"Cannot send a loan notification for status {loan.Status}.");
            }
            SendEmail(clientUser.Email, subject, body);
        }

        public void SendBanStatusNotification(ClientUser clientUser)
        {
            string subject;
            string body;
            if (clientUser.IsBanned)
            {
                subject = "Your ATM account has been banned";
                body =
                    $"Hello {clientUser.name},\n\n" +
                    "Your ATM account has been banned. You can no longer log in or use your accounts.\n" +
                    "If you think this is a mistake, please contact the bank.";
            }
            else
            {
                subject = "Your ATM account has been unbanned";
                body =
                    $"Hello {clientUser.name},\n\n" +
                    "Your ATM account has been unbanned. You can log in and use your accounts again.";
            }
            SendEmail(clientUser.Email, subject, body);
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            using var message = new MailMessage(new MailAddress(SenderAddress, SenderName), new MailAddress(toEmail))
            {
                Subject = subject,
                Body = body
            };
            using var client = new SmtpClient(SmtpHost, SmtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(SenderAddress, AppPassword)
            };
            client.Send(message);
        }
    }
}
