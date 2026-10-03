using ATM.Core.Enums;

namespace ATM.Core.Models
{
    public class ClientUser : User
    {
        public ClientUser()
        {
        }

        public ClientUser(Account account, LoanStatus status, decimal salary)
        {
            Accounts = new List<Account>();
            Loan = new Loans();
            Salary = salary;
        }

        public List<Account> Accounts { get; set; } = new List<Account>();

        public Loans Loan{ get; set; } = new Loans();

        public decimal Salary { get; set; }

        public string Email { get; set; } = string.Empty;

        public bool IsBanned { get; set; }

        // After a loan is approved or rejected, updating the salary is what allows a new loan request.
        public bool SalaryUpdatedSinceLastLoan { get; set; }

        public decimal TotalBalance()
        {
            return Accounts.Sum(a => a.Balance);
        }

    }
}
