namespace ATM.Core.Models
{
    public class Account
    {
        public Account()
        {
        }

        public Account(string accountNumber, decimal balance)
        {
            AccountNumber = accountNumber;
            Balance = balance;
        }

        // Always GE followed by 9 digits, for example GE123456789.
        public string AccountNumber { get; set; } = string.Empty;

        private decimal _money;

        public decimal Balance
        {
            get { return _money; }
            set { _money = value; }
        }

    }
}
