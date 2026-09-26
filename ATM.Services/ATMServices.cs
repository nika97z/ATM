using ATM.Core.Enums;
using ATM.Core.Exceptions;
using ATM.Core.Interfaces;
using ATM.Core.Models;

namespace ATM.Services
{
    public class ATMServices
    {
        private readonly Interface1 _repository;
        public ATMServices(Interface1 repository)
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
            _repository.Log($"User {clientUser.name} registered at {DateTime.Now} from IP: {IpService.GetIpAddress()}");

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
            _repository.Log($"Admin {adminUser.name} registered at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
        }
        public void AddAccountToClientUser(ClientUser clientUser)
        {
            Account newAccount = new Account();
            int acnum = new Random().Next(10000, 20000);
            while (_repository.GetAllClientUsers().Exists(u => u.Accounts != null && u.Accounts.Any(a => a.AccountNumber == acnum)))
            {
                acnum = new Random().Next(10000, 20000);
            }
            newAccount.AccountNumber = acnum;
            newAccount.Balance = 0m;
            clientUser.Accounts.Add(newAccount);
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"User {clientUser.name} added new account {newAccount.AccountNumber} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
        }
        public void AddMoneyToAccount(ClientUser clientUser, int accountNumber, decimal amount)
        {
            var account = clientUser.Accounts.Find(a => a.AccountNumber == accountNumber);
            if (account != null)
            {
                account.Balance += amount;
                _repository.UpdateClientUser(clientUser);
                _repository.Log($"User {clientUser.name} added {amount} gel to account {accountNumber} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
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
                    _repository.Log($"User {clientUser.name} subtracted {amount} gel from account {accountNumber} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
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
                    _repository.Log($"User {clientUser.name} transferred {amount} gel from account {accountNumber1} to account {accountNumber2} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
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
                    _repository.Log($"User {sender.name} transferred {amount} gel to user {receiver.name} (account {receiverAccountNumber}) at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
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
        public void RequestLoan(ClientUser clientUser, decimal amount, int time, int account)
        {
            if (clientUser.Loan.Status == LoanStatus.DidnotRequested)
            {
                clientUser.Loan.RequestedAmount = amount;
                clientUser.Loan.Time = time;
                clientUser.Loan.Account = account;
                clientUser.Loan.Status = LoanStatus.Pending;
                _repository.UpdateClientUser(clientUser);
                _repository.Log($"User {clientUser.name} requested a loan of {amount} gel for {time} months at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
                Console.WriteLine("Loan request submitted successfully.");
            }
            else
            {
                Console.WriteLine("You have already requested a loan or your loan is approved.");
            }
        }
        public List<ClientUser> ViewAllUsers()
        {
            return _repository.GetAllClientUsers();
        }
        public ClientUser FindClientUserByName(string name)
        {
            return _repository.GetAllClientUsers().Find(u => u.name == name);
        }
        public ClientUser FindClientUserByAccountNumber(int accountNumber)
        {
            return _repository.GetAllClientUsers().Find(u => u.Accounts.Exists(a => a.AccountNumber == accountNumber));
        }
        public ClientUser LoginUser()
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
                    string ipAddress = IpService.GetIpAddress();
                    _repository.Log($"User {clientUser.name} logged in at {DateTime.Now} from IP: {ipAddress}");
                    return clientUser;
                }
            }
        }
        public User LoginAdmin()
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
                    string ipAddress = IpService.GetIpAddress();
                    _repository.Log($"Admin {adminUser.name} logged in at {DateTime.Now} from IP: {ipAddress}");
                    return adminUser;
                }
            }
        }
        public void DeleteClientUser(ClientUser clientUser)
        {
            var UserAccounts = clientUser.Accounts;
            foreach (var account in UserAccounts)
            {
                if (account.Balance > 0)
                {
                    throw new DeleteAccountExtension();
                }
            }
            _repository.DeleteClientUser(clientUser);
            _repository.Log($"User {clientUser.name} deleted their account at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
        }
        public List<ClientUser> FindClientsByRequestedLoans()
        {
            return _repository.GetAllClientUsers().Where(u => u.Loan.Status == LoanStatus.Pending).ToList();
        }
        public List<ClientUser> FindClientsByApprovedLoans()
        {
            return _repository.GetAllClientUsers().Where(u => u.Loan.Status == LoanStatus.Approved).ToList();
        }
        public List<ClientUser> FindClientsByRejectedLoans()
        {
            return _repository.GetAllClientUsers().Where(u => u.Loan.Status == LoanStatus.Rejected).ToList();
        }
        public void ApproveLoan(ClientUser clientUser)
        {
            clientUser.Loan.Status = LoanStatus.Approved;
            var account = clientUser.Accounts.Find(a => a.AccountNumber == clientUser.Loan.Account);
            if (account != null)
            {
                account.Balance += clientUser.Loan.RequestedAmount;
                _repository.UpdateClientUser(clientUser);
                _repository.Log($"Loan approved for user {clientUser.name}, amount {clientUser.Loan.RequestedAmount} gel at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }
        public void RejectLoan(ClientUser clientUser)
        {
            clientUser.Loan.Status = LoanStatus.Rejected;
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"Loan rejected for user {clientUser.name} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
        }

    }
}
