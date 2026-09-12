using ATM.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
