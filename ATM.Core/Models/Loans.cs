using ATM.Core.Enums;

namespace ATM.Core.Models
{
    public class Loans
    {
        public Loans()
        {
        }

        public Loans(decimal amount, decimal requestedAmount, int time, LoanStatus status, string account)
        {
            Amount = amount;
            RequestedAmount = requestedAmount;
            Time = time;
            Status = status;
            Account = account;
        }

        public decimal Amount { get; set; } = 0m;
        public decimal RequestedAmount { get; set; } = 0m;
        public int Time { get; set; } = 0;
        public LoanStatus Status { get; set; } = LoanStatus.DidnotRequested;
        public string Account { get; set; } = string.Empty;
    }
}
