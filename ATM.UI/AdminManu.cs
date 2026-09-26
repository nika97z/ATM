using ATM.Core.Interfaces;
using ATM.Core.Models;
using ATM.Services;

namespace ATM.UI
{
    internal class AdminManu
    {
        private readonly ATMServices _services;
        private readonly Interface1 _repository;

        public AdminManu(ATMServices services, Interface1 repository)
        {
            _services = services;
            _repository = repository;
        }

        public void Show(ATMServices services, User adminUser)
        {
            Console.WriteLine("Welcome " + adminUser.name);
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("1 - View all users");
                Console.WriteLine("2 - Search for a user");
                Console.WriteLine("3 - View requested loans");
                Console.WriteLine("4 - View approved Loans");
                Console.WriteLine("5 - View rejected Loans");
                Console.WriteLine("6 - Exit");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        List<ClientUser> clientUsers = services.ViewAllUsers();
                        foreach (var user in clientUsers)
                        {
                            Console.WriteLine($"Name: {user.name}");
                            List<Account> accounts = user.Accounts;
                            foreach (var account in accounts)
                            {
                                Console.WriteLine($"  Account Number: {account.AccountNumber}, Balance: {account.Balance} gel");
                            }
                        }
                        break;
                    case "2":
                        Console.WriteLine("Enter the name of the user to search:");
                        string nameToSearch = Console.ReadLine();
                        Console.WriteLine();
                        ClientUser foundUser = services.FindClientUserByName(nameToSearch);
                        if (foundUser != null)
                        {
                            Console.WriteLine($"Name: {foundUser.name}");
                            List<Account> accounts = foundUser.Accounts;
                            foreach (var account in accounts)
                            {
                                Console.WriteLine($"Account Number: {account.AccountNumber}, Balance: {account.Balance} gel");
                            }
                        }
                        else
                        {
                            Console.WriteLine("User not found.");
                        }
                        break;
                    case "3":
                        Console.WriteLine("Requested Loans:");
                        List<ClientUser> Users = services.FindClientsByRequestedLoans();
                        if (Users.Count == 0)
                        {
                            Console.WriteLine("There are no requested loans.");
                            break;
                        }
                        for (int i = 0; i < Users.Count; i++)
                        {
                            var user = Users[i];
                            var cse = i + 1;
                            Console.WriteLine($"{cse} - User: {user.name}, Amount: {user.Loan.RequestedAmount} gel, Time: {user.Loan.Time} Month, Salary: {user.Salary} gel");
                        }
                        while (true)
                        {
                            Console.WriteLine("Choose User to Approve or Reject Loan (Enter number):");
                            string userChoice = Console.ReadLine();
                            if (int.TryParse(userChoice, out int userIndex) && userIndex >= 1 && userIndex <= Users.Count)
                            {
                                var selectedUser = Users[userIndex - 1];
                                Console.WriteLine($"Selected User: {selectedUser.name}");
                                Console.WriteLine("1 - Approve Loan");
                                Console.WriteLine("2 - Reject Loan");
                                string loanDecision = Console.ReadLine();
                                if (loanDecision == "1")
                                {
                                    services.ApproveLoan(selectedUser);
                                    Console.WriteLine("Loan approved.");
                                    break;
                                }
                                else if (loanDecision == "2")
                                {
                                    services.RejectLoan(selectedUser);
                                    Console.WriteLine("Loan rejected.");
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Invalid choice. Please try again.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid user number. Please try again.");
                            }
                        }
                        break;
                    case "4":
                        Console.WriteLine("Approved Loans:");
                        List<ClientUser> approvedUsers = services.FindClientsByApprovedLoans();
                        if (approvedUsers.Count == 0)
                        {
                            Console.WriteLine("There are no approved loans.");
                            break;
                        }
                        foreach (var user in approvedUsers)
                        {
                            Console.WriteLine($"User: {user.name}, Amount: {user.Loan.RequestedAmount} gel, Time: {user.Loan.Time} Month");
                        }
                        break;
                    case "5":
                        Console.WriteLine("Rejected Loans:");
                        List<ClientUser> rejectedUsers = services.FindClientsByRejectedLoans();
                        if (rejectedUsers.Count == 0)
                        {
                            Console.WriteLine("There are no rejected loans.");
                            break;
                        }
                        foreach (var user in rejectedUsers)
                        {
                            Console.WriteLine($"User: {user.name}, Amount: {user.Loan.RequestedAmount} gel, Time: {user.Loan.Time} Month");
                        }
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }
        
    }
}
