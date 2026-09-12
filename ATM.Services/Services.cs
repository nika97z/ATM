using ATM.Core.Enums;
using ATM.Core.Exceptions;
using ATM.Core.Interfaces;
using ATM.Core.Models;

namespace ATM.Services
{
    public class Services 
    {
        private readonly Interface1 _repository;
        public Services(Interface1 repository)
        {
            _repository = repository;
        }

        public void RegisterUser(ClientUser clientUser)
        {

            while (true)
            {
                Console.WriteLine("Enter your name:");
                string name = Console.ReadLine();
                string names = _repository.GetAllClientUsers().Find(u => u.name == name)?.name;
                if (names != null)
                {
                    Console.WriteLine("A user with that name already exists. Please choose a different name.");
                    continue;
                }
                Console.WriteLine("Enter your password:");
                string password = Console.ReadLine();
                Console.WriteLine("Enter your salary:");
                string salary = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(salary))
                {
                    Console.WriteLine("Name, password, and salary cannot be empty. Please try again.");
                    continue;
                }
       
                int id = _repository.GetAllClientUsers().Count + 1;
                while (_repository.GetAllClientUsers().Exists(u => u.id == id))
                {
                    id++;
                }
                clientUser.id = id;
                clientUser.name = name;
                clientUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
                try
                {
                    clientUser.Salary = decimal.Parse(salary);

                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid salary format. Please enter a valid decimal number.");
                    continue;
                }
                clientUser.Role = UserRole.User;
                int acnum = new Random().Next(10000, 20000);
                while (_repository.GetAllClientUsers().Exists(u => u.Accounts != null && u.Accounts.Any(a => a.AccountNumber == acnum)))
                {
                    acnum = new Random().Next(10000, 20000);
                }
                
                clientUser.Accounts.Add(new Account(acnum, 0m));
                Loans loan = new Loans();
                clientUser.Loan = loan;
                break;
            } 
             _repository.RegisterClientUser(clientUser);

        }

        public void RegisterAdminUser(AdminUser adminUser)
        {
            while (true)
            {
                Console.WriteLine("Enter your name:");
                string name = Console.ReadLine();
                string names = _repository.GetAllAdminUsers().Find(u => u.name == name)?.name;
                if (names != null)
                {
                    Console.WriteLine("A user with that name already exists. Please choose a different name.");
                    continue;
                }
                Console.WriteLine("Enter your password:");
                string password = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Name and password cannot be empty. Please try again.");
                    continue;
                }
                int id = _repository.GetAllAdminUsers().Count + 1;
                while (_repository.GetAllAdminUsers().Exists(u => u.id == id))
                {
                    id++;
                }
                adminUser.id = id;
                adminUser.name = name;
                adminUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
                adminUser.Role = UserRole.Admin;
                break;
            }
            _repository.RegisterAdminUser(adminUser);
        }

        public void AddAccountToClientUser(ClientUser clientUser)
        {
            Account newAccount = new Account();
            int acnum = new Random().Next(10000, 20000);
            newAccount.AccountNumber = acnum;
            newAccount.Balance = 0m;
            clientUser.Accounts.Add(newAccount);
            _repository.UpdateClientUser(clientUser);
        }

        public void AddMoneyToAccount(ClientUser clientUser, int accountNumber, decimal amount)
        {
            var account = clientUser.Accounts.Find(a => a.AccountNumber == accountNumber);
            if (account != null)
            {
                account.Balance += amount;
                _repository.UpdateClientUser(clientUser);
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }

        public void SubtractMoneyFromAccount(ClientUser clientUser, int accountNumber, decimal amount)
        {
            var account = clientUser.Accounts.Find(a => a.AccountNumber == accountNumber);
            if (account != null)
            {
                if (account.Balance >= amount)
                {
                    account.Balance -= amount;
                    _repository.UpdateClientUser(clientUser);
                }
                else
                {
                    throw new InsufficientFundsException("Insufficient funds for withdrawal.");
                }
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }
        public void TransferMoneyToOwnerAccount(ClientUser clientUser, int accountNumber1, int accountNumber2, decimal amount)
        {
            var account1 = clientUser.Accounts.Find(a => a.AccountNumber == accountNumber1);
            var account2 = clientUser.Accounts.Find(a => a.AccountNumber == accountNumber2);
            if (account1 != null && account2 != null)
            {
                if (account1.Balance >= amount)
                {
                    account1.Balance -= amount;
                    account2.Balance += amount;
                    _repository.UpdateClientUser(clientUser);
                }
                else
                {
                    throw new InsufficientFundsException("Insufficient funds for transfer.");
                }
            }
            else
            {
                Console.WriteLine("One or both accounts not found.");
            }
        }
        public void TransferMoneyToAnotherUser(ClientUser sender, ClientUser receiver, int senderAccountNumber, int receiverAccountNumber, decimal amount)
        {
            var senderAccount = sender.Accounts.Find(a => a.AccountNumber == senderAccountNumber);
            var receiverAccount = receiver.Accounts.Find(a => a.AccountNumber == receiverAccountNumber);
            if (senderAccount != null && receiverAccount != null)
            {
                if (senderAccount.Balance >= amount)
                {
                    senderAccount.Balance -= amount;
                    receiverAccount.Balance += amount;
                    _repository.UpdateClientUser(sender);
                    _repository.UpdateClientUser(receiver);
                }
                else
                {
                    throw new InsufficientFundsException("Insufficient funds for transfer.");
                }
            }
            else
            {
                Console.WriteLine("One or both accounts not found.");
            }
        }
        public void RequestLoan(ClientUser clientUser, decimal amount, int time)
        {
            if (clientUser.Loan.Status == LoanStatus.DidnotRequested)
            {
                clientUser.Loan.RequestedAmount = amount;
                clientUser.Loan.Time = time;
                clientUser.Loan.Status = LoanStatus.Pending;
                _repository.UpdateClientUser(clientUser);
                Console.WriteLine("Loan request submitted successfully.");
            }
            else
            {
                Console.WriteLine("You have already requested a loan or your loan is approved.");
            }
        }
        public void LoginUser()
        {
            while (true)
            {
                Console.WriteLine("Enter your name:");
                string name = Console.ReadLine();
                Console.WriteLine("Enter your password:");
                string password = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Name and password cannot be empty. Please try again.");
                    continue;
                }
                var clientUser = _repository.GetAll().Find(u => u.name == name && u.Role == UserRole.User) as ClientUser;
                if (clientUser == null)
                {
                    Console.WriteLine("User not found. Please try again.");
                    continue;
                }
                else if (!BCrypt.Net.BCrypt.Verify(password, clientUser.Password))
                {
                    Console.WriteLine("Invalid password. Please try again.");
                    continue;
                }
                else
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
                        Console.WriteLine($"Loan Requested: {clientUser.Loan.RequestedAmount}, Time: {clientUser.Loan.Time} months.");
                    }
                    else if (clientUser.Loan.Status == LoanStatus.Approved)
                    {
                        Console.WriteLine($"Your Loan Amount: {clientUser.Loan.RequestedAmount}, Time: {clientUser.Loan.Time} months.");
                    }
                    Console.WriteLine("Your accounts: ");
                    foreach (var account in clientUser.Accounts)
                    {
                        Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
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
                        Console.WriteLine("8 - Exit");
                        var choice = Console.ReadLine();
                        switch (choice)
                        {
                            case "1":
                                AddAccountToClientUser(clientUser);
                                Console.WriteLine("New account added successfully.");
                                foreach (var account in clientUser.Accounts)
                                {
                                    Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                }
                                break;
                            case "2":
                                Console.WriteLine("Enter amount to add:");
                                if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                                {
                                    if (clientUser.Accounts.Count == 1)
                                    {
                                        AddMoneyToAccount(clientUser, clientUser.Accounts[0].AccountNumber, amount);
                                        Console.WriteLine();
                                        Console.WriteLine("Money added succesfully");
                                        foreach (var account in clientUser.Accounts)
                                        {
                                            Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                        }
                                        Console.WriteLine();
                                    }

                                    else if (clientUser.Accounts.Count > 1)
                                    {
                                        Console.WriteLine("Choose Account To Add Money:");
                                        for (int i = 0; i < clientUser.Accounts.Count; i++)
                                        {
                                            Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance}");
                                        }
                                        if (int.TryParse(Console.ReadLine(), out int accountIndex) && accountIndex >= 1 && accountIndex <= clientUser.Accounts.Count)
                                        {
                                            AddMoneyToAccount(clientUser, clientUser.Accounts[accountIndex - 1].AccountNumber, amount);
                                            Console.WriteLine();
                                            Console.WriteLine("Money added succesfully");
                                            foreach (var account in clientUser.Accounts)
                                            {
                                                Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
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
                                } break;
                            case "3":
                                Console.WriteLine("Enter amount to subtract:");
                                if (decimal.TryParse(Console.ReadLine(), out decimal subtractAmount))
                                {
                                    if (clientUser.Accounts.Count == 1)
                                    {
                                        try
                                        {
                                            SubtractMoneyFromAccount(clientUser, clientUser.Accounts[0].AccountNumber, subtractAmount);
                                        }
                                        catch(InsufficientFundsException ex)
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine(ex.Message);
                                            Console.WriteLine();
                                            foreach (var account in clientUser.Accounts)
                                            {
                                                Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                            }
                                            break;
                                        }
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine();
                                            foreach (var account in clientUser.Accounts)
                                            {
                                                Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                            }
                                            break;
                                        }
                                    }
                                    else if (clientUser.Accounts.Count > 1)
                                    {
                                        Console.WriteLine("Choose Account To Subtract Money:");
                                        for (int i = 0; i < clientUser.Accounts.Count; i++)
                                        {
                                            Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance}");
                                        }
                                        if (int.TryParse(Console.ReadLine(), out int accountIndex) && accountIndex >= 1 && accountIndex <= clientUser.Accounts.Count)
                                        {
                                            try
                                            {
                                                SubtractMoneyFromAccount(clientUser, clientUser.Accounts[accountIndex - 1].AccountNumber, subtractAmount);
                                            }
                                            catch(InsufficientFundsException ex)
                                            {
                                                Console.WriteLine();
                                                Console.WriteLine(ex.Message);
                                                Console.WriteLine();
                                                foreach (var account in clientUser.Accounts)
                                                {
                                                    Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                                }
                                                break;
                                            }
                                            Console.WriteLine();
                                            Console.WriteLine("Money subtracted succesfully");
                                            foreach (var account in clientUser.Accounts)
                                            {
                                                Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
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
                                } break;
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
                                            Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance}");
                                        }
                                        if (int.TryParse(Console.ReadLine(), out int fromAccountIndex) && fromAccountIndex >= 1 && fromAccountIndex <= clientUser.Accounts.Count)
                                        {
                                            Console.WriteLine("Choose Account To Transfer Money To:");
                                            for (int i = 0; i < clientUser.Accounts.Count; i++)
                                            {
                                                if (i != fromAccountIndex - 1)
                                                {
                                                    Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance}");
                                                }
                                            }
                                            if (int.TryParse(Console.ReadLine(), out int toAccountIndex) && toAccountIndex >= 1 && toAccountIndex <= clientUser.Accounts.Count && toAccountIndex != fromAccountIndex)
                                            {
                                                try
                                                {
                                                    TransferMoneyToOwnerAccount(clientUser, clientUser.Accounts[fromAccountIndex - 1].AccountNumber, clientUser.Accounts[toAccountIndex - 1].AccountNumber, transferAmount);

                                                }
                                                catch (InsufficientFundsException ex)
                                                {
                                                    Console.WriteLine();
                                                    Console.WriteLine(ex.Message);
                                                    Console.WriteLine();
                                                    foreach (var account in clientUser.Accounts)
                                                    {
                                                        Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                                    }
                                                    break;
                                                }
                                                Console.WriteLine();
                                                Console.WriteLine("Money transferred succesfully");
                                                foreach (var account in clientUser.Accounts)
                                                {
                                                    Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
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
                                var receiver = _repository.GetAllClientUsers().Find(u => u.name == receiverName);
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
                                        Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance}");
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
                                                TransferMoneyToAnotherUser(clientUser, receiver, clientUser.Accounts[fromAccountIndex - 1].AccountNumber, receiver.Accounts[toAccountIndex - 1].AccountNumber, transferAmountToAnotherUser);
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
                                                    Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                                }
                                                break;
                                            }
                                            foreach (var account in clientUser.Accounts)
                                            {
                                                Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                            }
                                        }
                                    }
                                }
                                break;
                            case "6":
                                Console.WriteLine("Enter the account number of the user to transfer money to:");
                                if (int.TryParse(Console.ReadLine(), out int receiverAccountNumber))
                                {
                                    var receiverByAccount = _repository.GetAllClientUsers().Find(u => u.Accounts.Exists(a => a.AccountNumber == receiverAccountNumber));
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
                                            Console.WriteLine($"{i + 1} - Account Number: {clientUser.Accounts[i].AccountNumber}, Balance: {clientUser.Accounts[i].Balance}");
                                        }
                                        if (int.TryParse(Console.ReadLine(), out int fromAccountIndex) && fromAccountIndex >= 1 && fromAccountIndex <= clientUser.Accounts.Count)
                                        {
                                            try
                                            {
                                                TransferMoneyToAnotherUser(clientUser, receiverByAccount, clientUser.Accounts[fromAccountIndex - 1].AccountNumber, receiverAccountNumber, transferAmountToAnotherUserByAccount);
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
                                                    Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                                }
                                                break;
                                            }
                                            foreach (var account in clientUser.Accounts)
                                            {
                                                Console.WriteLine("- Account Number: " + account.AccountNumber + ", Balance: " + account.Balance);
                                            }
                                        }
                                    }
                                }
                                break;
                            case "7":
                                Console.WriteLine("Enter the amount you want to request for the loan:");
                                if (decimal.TryParse(Console.ReadLine(), out decimal loanAmount))
                                {
                                    Console.WriteLine("Enter the time period for the loan in Months:");
                                    if (int.TryParse(Console.ReadLine(), out int loanTime))
                                        if (loanTime > 0 && loanTime <= 48)
                                        {
                                            RequestLoan(clientUser, loanAmount, loanTime);

                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid time period. Please enter a value between 1 and 48 months.");
                                        }
                                    else
                                    {
                                        Console.WriteLine("Invalid time format.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid amount format.");
                                }
                                break;
                            case "8":
                                Console.WriteLine("Exiting...");
                                return;
                            default:
                                Console.WriteLine("Invalid choice.");
                                break;
                        }
                    }
                }
            }
        }

        public void LoginAdmin()
        {
            while (true)
            {
                Console.WriteLine("Enter your name:");
                string name = Console.ReadLine();
                Console.WriteLine("Enter your password:");
                string password = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Name and password cannot be empty. Please try again.");
                    continue;
                }
                var adminUser = _repository.GetAll().Find(u => u.name == name && u.Role == UserRole.Admin);
                if (adminUser == null)
                {
                    Console.WriteLine("Admin not found. Please try again.");
                    continue;
                }
                else if (!BCrypt.Net.BCrypt.Verify(password, adminUser.Password))
                {
                    Console.WriteLine("Invalid password. Please try again.");
                    continue;
                }
                else
                {
                    Console.WriteLine("Login successful.");
                    break;
                }
            }
        }
    }
}

