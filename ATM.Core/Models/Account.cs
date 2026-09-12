using System;
using System.Collections.Generic;
using System.Text;

namespace ATM.Core.Models
{
    public class Account
    {
        public Account()
        {
        }

        public Account(int accountNumber, decimal balance)
        {
            AccountNumber = accountNumber;
            Balance = balance;
        }
        public int AccountNumber { get; set; }

        private decimal _money;

        public decimal Balance
        {
            get { return _money; }
            set { _money = value; }
        }

    }
}
