using ATM.Core.Enums;
using ATM.Core.Exceptions;
using ATM.Core.Interfaces;
using ATM.Core.Models;
using System.Net.Mail;
using System.Security.Authentication;
using System.Security.Cryptography;

namespace ATM.Services
{
    public class ATMServices
    {
        private const int VerificationCodeAttempts = 3;
        private static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromMinutes(5);

        private readonly Interface1 _repository;
        private readonly IEmailService _emailService;
        public ATMServices(Interface1 repository, IEmailService emailService)
        {
            _repository = repository;
            _emailService = emailService;
        }


        public bool IsClientNameTaken(string name)
        {
            return _repository.GetAllClientUsers().Exists(u => u.name == name);
        }
        public bool IsAdminNameTaken(string name)
        {
            return _repository.GetAllAdminUsers().Exists(u => u.name == name);
        }
        public bool IsEmailTaken(string email)
        {
            return _repository.GetAllClientUsers().Exists(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        }
        public static bool IsValidEmail(string email)
        {
            // Reject display-name forms like "Nika <nika@gmail.com>"; only a bare address is accepted.
            return MailAddress.TryCreate(email, out var address) && address.Address == email;
        }

        public const string NameRule = "Name must be at least 3 characters long.";
        public const string PasswordRule = "Password must be at least 8 characters long and contain at least one number.";
        public const string AccountNumberRule = "Account number must be GE followed by 9 digits, for example GE123456789.";
        public const string SalaryRule = "Monthly salary cannot be negative.";

        public static bool IsValidName(string name)
        {
            return name.Trim().Length >= 3;
        }
        public static bool IsValidPassword(string password)
        {
            return password.Length >= 8 && password.Any(char.IsAsciiDigit);
        }
        public static bool IsValidAccountNumber(string accountNumber)
        {
            return accountNumber.Length == 11
                && accountNumber.StartsWith("GE", StringComparison.Ordinal)
                && accountNumber.Skip(2).All(char.IsAsciiDigit);
        }
        private string GenerateAccountNumber()
        {
            var takenNumbers = _repository.GetAllClientUsers()
                .Where(u => u.Accounts != null)
                .SelectMany(u => u.Accounts)
                .Select(a => a.AccountNumber)
                .ToHashSet();
            string accountNumber;
            do
            {
                accountNumber = $"GE{Random.Shared.Next(0, 1000000000):D9}";
            }
            while (takenNumbers.Contains(accountNumber));
            return accountNumber;
        }

        // Builds the new client and emails a verification code. Nothing is saved until
        // CompleteUserRegistration accepts the code.
        public PendingRegistration StartUserRegistration(string name, string password, decimal salary, string email)
        {
            if (!IsValidName(name))
            {
                throw new ArgumentException(NameRule);
            }
            if (!IsValidPassword(password))
            {
                throw new ArgumentException(PasswordRule);
            }
            if (salary < 0)
            {
                throw new ArgumentException(SalaryRule);
            }
            if (IsClientNameTaken(name))
            {
                throw new ArgumentException("A user with that name already exists. Please choose a different name.");
            }
            if (!IsValidEmail(email))
            {
                throw new ArgumentException("Invalid email address.");
            }
            if (IsEmailTaken(email))
            {
                throw new ArgumentException("A user with that email already exists. Please use a different email.");
            }

            ClientUser clientUser = new ClientUser();
            int id = _repository.GetAllClientUsers().Count + 1;
            while (_repository.GetAllClientUsers().Exists(u => u.id == id))
            {
                id++;
            }
            clientUser.id = id;
            clientUser.name = name;
            clientUser.Email = email;
            clientUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
            clientUser.Salary = salary;
            clientUser.Role = UserRole.User;
            clientUser.Accounts.Add(new Account(GenerateAccountNumber(), 0m));
            Loans loan = new Loans();
            clientUser.Loan = loan;

            string code = RandomNumberGenerator.GetInt32(0, 10000).ToString("D4");
            DateTime expiresAt = DateTime.Now.Add(VerificationCodeLifetime);
            try
            {
                _emailService.SendVerificationCode(clientUser.Email, clientUser.name, code);
            }
            catch (Exception ex) when (ex is SmtpException || ex is InvalidOperationException)
            {
                throw new EmailVerificationException($"Could not send verification email: {ex.Message}");
            }
            return new PendingRegistration(clientUser, code, expiresAt, VerificationCodeAttempts);
        }

        // Returns true and saves the client when the code is correct, or false when it is wrong
        // but attempts remain. Throws once the code has expired or the last attempt is used up.
        public bool CompleteUserRegistration(PendingRegistration registration, string enteredCode)
        {
            if (DateTime.Now > registration.ExpiresAt)
            {
                throw new EmailVerificationException("Verification code has expired. Please register again.");
            }
            if (enteredCode != registration.Code)
            {
                registration.AttemptsLeft--;
                if (registration.AttemptsLeft <= 0)
                {
                    throw new EmailVerificationException("Too many incorrect codes. Email was not verified. Please register again.");
                }
                return false;
            }
            ClientUser clientUser = registration.ClientUser;
            _repository.Log($"User {clientUser.name} verified email {clientUser.Email} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
            _repository.RegisterClientUser(clientUser);
            _repository.Log($"User {clientUser.name} registered at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
            return true;
        }
        // Returns the new admin, so the caller can log them in straight away.
        public AdminUser RegisterAdminUser(string name, string password)
        {
            if (!IsValidName(name))
            {
                throw new ArgumentException(NameRule);
            }
            if (!IsValidPassword(password))
            {
                throw new ArgumentException(PasswordRule);
            }
            if (IsAdminNameTaken(name))
            {
                throw new ArgumentException("A user with that name already exists. Please choose a different name.");
            }
            AdminUser adminUser = new AdminUser();
            int id = _repository.GetAllAdminUsers().Count + 1;
            while (_repository.GetAllAdminUsers().Exists(u => u.id == id))
            {
                id++;
            }
            adminUser.id = id;
            adminUser.name = name;
            adminUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
            adminUser.Role = UserRole.Admin;
            _repository.RegisterAdminUser(adminUser);
            _repository.Log($"Admin {adminUser.name} registered at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
            return adminUser;
        }
        public void UpdateSalary(ClientUser clientUser, decimal newSalary)
        {
            if (newSalary < 0)
            {
                throw new ArgumentException(SalaryRule);
            }
            decimal oldSalary = clientUser.Salary;
            clientUser.Salary = newSalary;
            clientUser.SalaryUpdatedSinceLastLoan = true;
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"User {clientUser.name} updated monthly salary from {oldSalary} to {newSalary} gel at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
        }
        public void AddAccountToClientUser(ClientUser clientUser)
        {
            Account newAccount = new Account(GenerateAccountNumber(), 0m);
            clientUser.Accounts.Add(newAccount);
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"User {clientUser.name} added new account {newAccount.AccountNumber} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
        }
        public void DeleteAccountFromClientUser(ClientUser clientUser, string accountNumber)
        {
            var account = clientUser.Accounts.Find(a => a.AccountNumber == accountNumber);
            if (account == null)
            {
                throw new InvalidOperationException("Account not found.");
            }
            if (clientUser.Accounts.Count <= 1)
            {
                throw new DeleteAccountExtension("You cannot delete your only account.");
            }
            if (account.Balance != 0)
            {
                throw new DeleteAccountExtension("Cannot delete an account that still has money in it. Withdraw or transfer the money first.");
            }
            // Approving the loan would otherwise fail because the money has nowhere to go.
            if (clientUser.Loan.Status == LoanStatus.Pending && clientUser.Loan.Account == accountNumber)
            {
                throw new DeleteAccountExtension("This account is waiting to receive a loan, so it cannot be deleted.");
            }
            clientUser.Accounts.Remove(account);
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"User {clientUser.name} deleted account {accountNumber} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
        }
        public void AddMoneyToAccount(ClientUser clientUser, string accountNumber, decimal amount)
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
                throw new InvalidOperationException("Account not found.");
            }
        }
        public void SubtractMoneyFromAccount(ClientUser clientUser, string accountNumber, decimal amount)
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
                throw new InvalidOperationException("Account not found.");
            }
        }
        public void TransferMoneyToOwnerAccount(ClientUser clientUser, string accountNumber1, string accountNumber2, decimal amount)
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
                throw new InvalidOperationException("One or both accounts not found.");
            }
        }
        public void TransferMoneyToAnotherUser(ClientUser sender, ClientUser receiver, string senderAccountNumber, string receiverAccountNumber, decimal amount)
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
                throw new InvalidOperationException("One or both accounts not found.");
            }
        }
        // Returns why the client cannot request a loan right now, or null when they can.
        public static string? LoanRequestBlockedReason(ClientUser clientUser)
        {
            if (clientUser.Loan.Status == LoanStatus.DidnotRequested)
            {
                return null;
            }
            if (clientUser.Loan.Status == LoanStatus.Pending)
            {
                return "Your previous loan request is still pending. You can request a new loan once it is approved or rejected.";
            }
            if (!clientUser.SalaryUpdatedSinceLastLoan)
            {
                return "To request a new loan, first update your monthly salary.";
            }
            return null;
        }
        // A new request replaces the previous (approved or rejected) loan on the client.
        public void RequestLoan(ClientUser clientUser, decimal amount, int time, string account)
        {
            string? blockedReason = LoanRequestBlockedReason(clientUser);
            if (blockedReason != null)
            {
                throw new LoanExeption(blockedReason);
            }
            clientUser.Loan = new Loans
            {
                RequestedAmount = amount,
                Time = time,
                Account = account,
                Status = LoanStatus.Pending
            };
            clientUser.SalaryUpdatedSinceLastLoan = false;
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"User {clientUser.name} requested a loan of {amount} gel for {time} months at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
        }
        public List<ClientUser> ViewAllUsers()
        {
            return _repository.GetAllClientUsers();
        }
        public ClientUser FindClientUserByName(string name)
        {
            return _repository.GetAllClientUsers().Find(u => u.name == name);
        }
        public ClientUser FindClientUserByAccountNumber(string accountNumber)
        {
            return _repository.GetAllClientUsers().Find(u => u.Accounts.Exists(a => a.AccountNumber == accountNumber));
        }
        public ClientUser LoginUser(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Name and password cannot be empty.");
            }
            var clientUser = _repository.GetAll().Find(u => u.name == name && u.Role == UserRole.User) as ClientUser;
            if (clientUser == null)
            {
                throw new AuthenticationException("User not found.");
            }
            if (!BCrypt.Net.BCrypt.Verify(password, clientUser.Password))
            {
                throw new AuthenticationException("Invalid password.");
            }
            string ipAddress = IpService.GetIpAddress();
            // Checked after the password, so only the account owner learns that it is banned.
            if (clientUser.IsBanned)
            {
                _repository.Log($"Banned user {clientUser.name} tried to log in at {DateTime.Now} from IP: {ipAddress}");
                throw new UserBannedException("This user is banned. Please contact the bank for more information.");
            }
            _repository.Log($"User {clientUser.name} logged in at {DateTime.Now} from IP: {ipAddress}");
            return clientUser;
        }
        public User LoginAdmin(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Name and password cannot be empty.");
            }
            var adminUser = _repository.GetAll().Find(u => u.name == name && u.Role == UserRole.Admin);
            if (adminUser == null)
            {
                throw new AuthenticationException("Admin not found.");
            }
            if (!BCrypt.Net.BCrypt.Verify(password, adminUser.Password))
            {
                throw new AuthenticationException("Invalid password.");
            }
            string ipAddress = IpService.GetIpAddress();
            _repository.Log($"Admin {adminUser.name} logged in at {DateTime.Now} from IP: {ipAddress}");
            return adminUser;
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
        // Returns a message saying whether the client was notified by email.
        public string ApproveLoan(ClientUser clientUser)
        {
            clientUser.Loan.Status = LoanStatus.Approved;
            var account = clientUser.Accounts.Find(a => a.AccountNumber == clientUser.Loan.Account);
            if (account != null)
            {
                account.Balance += clientUser.Loan.RequestedAmount;
                _repository.UpdateClientUser(clientUser);
                _repository.Log($"Loan approved for user {clientUser.name}, amount {clientUser.Loan.RequestedAmount} gel at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
                return NotifyByEmail(clientUser, $"Loan {clientUser.Loan.Status}", () => _emailService.SendLoanStatusNotification(clientUser));
            }
            else
            {
                throw new InvalidOperationException("Account not found.");
            }
        }
        // Returns a message saying whether the client was notified by email.
        public string RejectLoan(ClientUser clientUser)
        {
            clientUser.Loan.Status = LoanStatus.Rejected;
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"Loan rejected for user {clientUser.name} at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
            return NotifyByEmail(clientUser, $"Loan {clientUser.Loan.Status}", () => _emailService.SendLoanStatusNotification(clientUser));
        }
        // Returns a message saying whether the client was notified by email.
        public string BanClientUser(ClientUser clientUser)
        {
            clientUser.IsBanned = true;
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"User {clientUser.name} was banned at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
            return NotifyByEmail(clientUser, "Ban", () => _emailService.SendBanStatusNotification(clientUser));
        }
        // Returns a message saying whether the client was notified by email.
        public string UnbanClientUser(ClientUser clientUser)
        {
            clientUser.IsBanned = false;
            _repository.UpdateClientUser(clientUser);
            _repository.Log($"User {clientUser.name} was unbanned at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
            return NotifyByEmail(clientUser, "Unban", () => _emailService.SendBanStatusNotification(clientUser));
        }
        private string NotifyByEmail(ClientUser clientUser, string emailKind, Action sendEmail)
        {
            if (string.IsNullOrWhiteSpace(clientUser.Email))
            {
                return $"User {clientUser.name} has no email address, so no notification was sent.";
            }
            // The decision is already saved, so a failed email must not undo or abort it.
            try
            {
                sendEmail();
                _repository.Log($"{emailKind} email sent to user {clientUser.name} ({clientUser.Email}) at {DateTime.Now} from IP: {IpService.GetIpAddress()}");
                return $"Notification email sent to {clientUser.Email}.";
            }
            catch (Exception ex)
            {
                _repository.Log($"Failed to send {emailKind} email to user {clientUser.name} ({clientUser.Email}): {ex.Message}");
                return $"Could not send notification email: {ex.Message}";
            }
        }

    }
}
