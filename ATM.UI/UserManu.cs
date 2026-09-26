using ATM.Core.Enums;
using ATM.Core.Exceptions;
using ATM.Core.Interfaces;
using ATM.Core.Models;
using ATM.Services;

namespace ATM.UI
{
    internal class UserManu
    {
        private readonly ATMServices _services;
        private readonly Interface1 _repository;

        public UserManu(ATMServices services, Interface1 repository)
        {
            _services = services;
            _repository = repository;
        }

        public void Show(ATMServices services, ClientUser clientUser)
        {
            Console.WriteLine("Welcome, " + clientUser.name + "!");
            Console.WriteLine();
            Console.WriteLine("your Loans: ");
            if (clientUser.Loan.Status == LoanStatus.DidnotRequested)
            {
                Console.WriteLine("You have not requested any loans.");
            }
            else if (clientUser.Loan.Status == LoanStatus.Pending)
            {
                Console.WriteLine($"Loan Requested: {clientUser.Loan.RequestedAmount} gel, Time: {clientUser.Loan.Time} months.");
            }
            else if (clientUser.Loan.Status == LoanStatus.Approved)
            {
                Console.WriteLine($"Your Loan Amount: {clientUser.Loan.RequestedAmount} gel, Time: {clientUser.Loan.Time} months.");
            }
            Console.WriteLine("Your accounts: ");
            foreach (var account in clientUser.Accounts)
            {
                Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
            }
            while (true)
            {
                Console.WriteLine("1 - Add new Account");
                Console.WriteLine("2 - Add Money to Account");
                Console.WriteLine("3 - Subtract Money from Account");
                Console.WriteLine("4 - Transfer Money to Your Account");
                Console.WriteLine("5 - Transfer Money to Another Account by name");
                Console.WriteLine("6 - Transfer Money to Another Account by Account Number");
                Console.WriteLine("7 - Request Loan");
                Console.WriteLine("8 - Delete Account");
                Console.WriteLine("9 - Exit");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        services.AddAccountToClientUser(clientUser);
                        Console.WriteLine("New account added successfully.");
                        foreach (var account in clientUser.Accounts)
                        {
                            Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                        }
                        break;
                    case "2":
                        Console.WriteLine("Enter amount to add:");
                        if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0 && amount <= 10000000000)
                        {
                            if (clientUser.Accounts.Count == 1)
                            {
                                services.AddMoneyToAccount(clientUser, clientUser.Accounts[0].AccountNumber, amount);
                                Console.WriteLine();
                                Console.WriteLine("Money added succesfully");
                                foreach (var account in clientUser.Accounts)
                                {
                                    Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                }
                                Console.WriteLine();
                            }

                            else if (clientUser.Accounts.Count > 1)
                            {
                                Console.WriteLine("Choose Account To Add Money:");
                                for (int i = 0; i < clientUser.Accounts.Count; i++)
                                {
                                    Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance} gel");
                                }
                                if (int.TryParse(Console.ReadLine(), out int accountIndex) && accountIndex >= 1 && accountIndex <= clientUser.Accounts.Count)
                                {
                                    services.AddMoneyToAccount(clientUser, clientUser.Accounts[accountIndex - 1].AccountNumber, amount);
                                    Console.WriteLine();
                                    Console.WriteLine("Money added succesfully");
                                    foreach (var account in clientUser.Accounts)
                                    {
                                        Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                    }
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Console.WriteLine("Invalid account selection.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid account number format.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                        }
                        break;
                    case "3":
                        Console.WriteLine("Enter amount to subtract:");
                        if (decimal.TryParse(Console.ReadLine(), out decimal subtractAmount))
                        {
                            if (clientUser.Accounts.Count == 1)
                            {
                                try
                                {
                                    services.SubtractMoneyFromAccount(clientUser, clientUser.Accounts[0].AccountNumber, subtractAmount);
                                }
                                catch (InsufficientFundsException ex)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine();
                                    foreach (var account in clientUser.Accounts)
                                    {
                                        Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                    }
                                    break;
                                }
                                {
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    foreach (var account in clientUser.Accounts)
                                    {
                                        Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                    }
                                    break;
                                }
                            }
                            else if (clientUser.Accounts.Count > 1)
                            {
                                Console.WriteLine("Choose Account To Subtract Money:");
                                for (int i = 0; i < clientUser.Accounts.Count; i++)
                                {
                                    Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance} gel");
                                }
                                if (int.TryParse(Console.ReadLine(), out int accountIndex) && accountIndex >= 1 && accountIndex <= clientUser.Accounts.Count)
                                {
                                    try
                                    {
                                        services.SubtractMoneyFromAccount(clientUser, clientUser.Accounts[accountIndex - 1].AccountNumber, subtractAmount);
                                    }
                                    catch (InsufficientFundsException ex)
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine(ex.Message);
                                        Console.WriteLine();
                                        foreach (var account in clientUser.Accounts)
                                        {
                                            Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                        }
                                        break;
                                    }
                                    Console.WriteLine();
                                    Console.WriteLine("Money subtracted succesfully");
                                    foreach (var account in clientUser.Accounts)
                                    {
                                        Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                    }
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Console.WriteLine("Invalid account selection.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid account number format.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount format.");
                        }
                        break;
                    case "4":
                        Console.WriteLine("Enter amount to transfer:");
                        if (decimal.TryParse(Console.ReadLine(), out decimal transferAmount))
                        {
                            if (clientUser.Accounts.Count == 1)
                            {
                                Console.WriteLine("You have only one account. Cannot transfer money.");
                            }
                            else if (clientUser.Accounts.Count > 1)
                            {
                                Console.WriteLine("Choose Account To Transfer Money From:");
                                for (int i = 0; i < clientUser.Accounts.Count; i++)
                                {
                                    Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance} gel");
                                }
                                if (int.TryParse(Console.ReadLine(), out int fromAccountIndex) && fromAccountIndex >= 1 && fromAccountIndex <= clientUser.Accounts.Count)
                                {
                                    Console.WriteLine("Choose Account To Transfer Money To:");
                                    for (int i = 0; i < clientUser.Accounts.Count; i++)
                                    {
                                        if (i != fromAccountIndex - 1)
                                        {
                                            Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance} gel");
                                        }
                                    }
                                    if (int.TryParse(Console.ReadLine(), out int toAccountIndex) && toAccountIndex >= 1 && toAccountIndex <= clientUser.Accounts.Count && toAccountIndex != fromAccountIndex)
                                    {
                                        try
                                        {
                                            services.TransferMoneyToOwnerAccount(clientUser, clientUser.Accounts[fromAccountIndex - 1].AccountNumber, clientUser.Accounts[toAccountIndex - 1].AccountNumber, transferAmount);

                                        }
                                        catch (InsufficientFundsException ex)
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine(ex.Message);
                                            Console.WriteLine();
                                            foreach (var account in clientUser.Accounts)
                                            {
                                                Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                            }
                                            break;
                                        }
                                        Console.WriteLine();
                                        Console.WriteLine("Money transferred succesfully");
                                        foreach (var account in clientUser.Accounts)
                                        {
                                            Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                        }
                                        Console.WriteLine();
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid account selection.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid account selection.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid account number format.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount format.");
                        }
                        break;
                    case "5":
                        Console.WriteLine("Enter the name of the user to transfer money to:");
                        string receiverName = Console.ReadLine();
                        var receiver = services.FindClientUserByName(receiverName);
                        if (receiver == null)
                        {
                            Console.WriteLine("User not found.");
                            break;
                        }
                        if (receiver.id == clientUser.id)
                        {
                            Console.WriteLine("You cannot transfer money to your own account this way. Use option 4 instead.");
                            break;
                        }
                        Console.WriteLine("Enter amount to transfer:");
                        if (decimal.TryParse(Console.ReadLine(), out decimal transferAmountToAnotherUser))
                        {
                            if (clientUser.Accounts.Count == 0)
                            {
                                Console.WriteLine("You have no accounts. Cannot transfer money.");
                                break;
                            }
                            Console.WriteLine("Choose Account To Transfer Money From:");
                            for (int i = 0; i < clientUser.Accounts.Count; i++)
                            {
                                Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance} gel");
                            }
                            if (int.TryParse(Console.ReadLine(), out int fromAccountIndex) && fromAccountIndex >= 1 && fromAccountIndex <= clientUser.Accounts.Count)
                            {
                                if (receiver.Accounts.Count == 0)
                                {
                                    Console.WriteLine("The receiver has no accounts. Cannot transfer money.");
                                    break;
                                }
                                Console.WriteLine("Choose Account To Transfer Money To:");
                                for (int i = 0; i < receiver.Accounts.Count; i++)
                                {
                                    Console.WriteLine($"{i + 1} - Account Number: {receiver.Accounts[i].AccountNumber}");
                                }
                                if (int.TryParse(Console.ReadLine(), out int toAccountIndex) && toAccountIndex >= 1 && toAccountIndex <= receiver.Accounts.Count)
                                {
                                    try
                                    {
                                        services.TransferMoneyToAnotherUser(clientUser, receiver, clientUser.Accounts[fromAccountIndex - 1].AccountNumber, receiver.Accounts[toAccountIndex - 1].AccountNumber, transferAmountToAnotherUser);
                                        Console.WriteLine();
                                        Console.WriteLine("Money transferred succesfully");
                                    }
                                    catch (InsufficientFundsException ex)
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine(ex.Message);
                                        Console.WriteLine();
                                        foreach (var account in clientUser.Accounts)
                                        {
                                            Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                        }
                                        break;
                                    }
                                    foreach (var account in clientUser.Accounts)
                                    {
                                        Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                    }
                                }
                            }
                        }
                        break;
                    case "6":
                        Console.WriteLine("Enter the account number of the user to transfer money to:");
                        if (int.TryParse(Console.ReadLine(), out int receiverAccountNumber))
                        {
                            var receiverByAccount = services.FindClientUserByAccountNumber(receiverAccountNumber);
                            if (receiverByAccount == null)
                            {
                                Console.WriteLine("User with that account number not found.");
                                break;
                            }
                            if (receiverByAccount.id == clientUser.id)
                            {
                                Console.WriteLine("You cannot transfer money to your own account this way. Use option 4 instead.");
                                break;
                            }
                            Console.WriteLine("Enter amount to transfer:");
                            if (decimal.TryParse(Console.ReadLine(), out decimal transferAmountToAnotherUserByAccount))
                            {
                                if (clientUser.Accounts.Count == 0)
                                {
                                    Console.WriteLine("You have no accounts. Cannot transfer money.");
                                    break;
                                }
                                Console.WriteLine("Choose Account To Transfer Money From:");
                                for (int i = 0; i < clientUser.Accounts.Count; i++)
                                {
                                    Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance} gel");
                                }
                                if (int.TryParse(Console.ReadLine(), out int fromAccountIndex) && fromAccountIndex >= 1 && fromAccountIndex <= clientUser.Accounts.Count)
                                {
                                    try
                                    {
                                        services.TransferMoneyToAnotherUser(clientUser, receiverByAccount, clientUser.Accounts[fromAccountIndex - 1].AccountNumber, receiverAccountNumber, transferAmountToAnotherUserByAccount);
                                        Console.WriteLine();
                                        Console.WriteLine("Money transferred succesfully");
                                        Console.WriteLine();
                                    }
                                    catch (InsufficientFundsException ex)
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine(ex.Message);
                                        Console.WriteLine();
                                        foreach (var account in clientUser.Accounts)
                                        {
                                            Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                        }
                                        break;
                                    }
                                    foreach (var account in clientUser.Accounts)
                                    {
                                        Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance + " gel");
                                    }
                                }
                            }
                        }
                        break;
                    case "7":
                        try
                        {
                            Console.WriteLine("Enter the amount you want to request for the loan:");
                            if (decimal.TryParse(Console.ReadLine(), out decimal loanAmount) && loanAmount > 0 && loanAmount <= 10000000000)
                            {
                                if (loanAmount <= 400)
                                {
                                    throw new LoanExeption("Loan amount must be greater than 400 gel.");
                                }
                                Console.WriteLine("Enter the time period for the loan in Months:");
                                if (int.TryParse(Console.ReadLine(), out int loanTime))
                                    if (loanTime > 4 && loanTime <= 48)
                                    {
                                        int loanAccountNumber;
                                        if (clientUser.Accounts.Count == 1)
                                        {
                                            loanAccountNumber = clientUser.Accounts[0].AccountNumber;
                                            services.RequestLoan(clientUser, loanAmount, loanTime, loanAccountNumber);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Choose Account To Receive The Loan:");
                                            for (int i = 0; i < clientUser.Accounts.Count; i++)
                                            {
                                                Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance} gel");
                                            }
                                            if (int.TryParse(Console.ReadLine(), out int loanAccountIndex) && loanAccountIndex >= 1 && loanAccountIndex <= clientUser.Accounts.Count)
                                            {
                                                loanAccountNumber = clientUser.Accounts[loanAccountIndex - 1].AccountNumber;
                                                services.RequestLoan(clientUser, loanAmount, loanTime, loanAccountNumber);
                                            }
                                            else
                                            {
                                                Console.WriteLine("Invalid account selection.");
                                            }
                                        }

                                    }
                                    else
                                    {
                                        throw new LoanExeption("Loan time must be between 5 and 48 months.");
                                    }
                                else
                                {
                                    Console.WriteLine("Invalid time format.");
                                }
                            }
                            else
                            {
                                throw new FormatException("Invalid amount format.");
                            }
                        }
                        catch (LoanExeption ex)
                        {
                            Console.WriteLine();
                            Console.WriteLine(ex.Message);
                            Console.WriteLine();
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine();
                            Console.WriteLine(ex.Message);
                            Console.WriteLine();
                        }
                        break;
                    case "8":
                        try
                        {
                            services.DeleteClientUser(clientUser);
                            Console.WriteLine("Your account has been deleted successfully.");
                            return;
                        }
                        catch (DeleteAccountExtension ex)
                        {
                            Console.WriteLine();
                            Console.WriteLine(ex.Message);
                            Console.WriteLine();
                            break;
                        }
                    case "9":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }
    }
}
