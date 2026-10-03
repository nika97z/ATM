using ATM.Core.Enums;
using ATM.Core.Models;
using ATM.Services;
using Spectre.Console;

namespace ATM.UI
{
    internal class AdminManu
    {
        private const string ViewUsers = "View all users";
        private const string SearchUser = "Search for a user";
        private const string ReviewLoans = "Review requested loans";
        private const string ViewApproved = "View approved loans";
        private const string ViewRejected = "View rejected loans";
        private const string BanUser = "Ban or unban a user";
        private const string ViewLogs = "View logs";
        private const string LogOut = "Log out";

        private readonly ATMServices _services;

        public AdminManu(ATMServices services)
        {
            _services = services;
        }

        public void Show(User adminUser)
        {
            while (true)
            {
                ConsoleUi.ShowTitle($"Admin panel - {adminUser.name}");
                int pendingCount = _services.FindClientsByRequestedLoans().Count;
                if (pendingCount > 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]{pendingCount} loan request(s) waiting for review.[/]");
                    AnsiConsole.WriteLine();
                }
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What would you like to do?")
                        .AddChoices(ViewUsers, SearchUser, ReviewLoans, ViewApproved, ViewRejected, BanUser, ViewLogs, LogOut));
                if (choice == LogOut)
                {
                    return;
                }

                try
                {
                    switch (choice)
                    {
                        case ViewUsers:
                            ShowAllUsers();
                            break;
                        case SearchUser:
                            SearchForUser();
                            break;
                        case ReviewLoans:
                            ReviewLoanRequests();
                            break;
                        case ViewApproved:
                            ShowLoans("Approved loans", _services.FindClientsByApprovedLoans(), "There are no approved loans.");
                            break;
                        case ViewRejected:
                            ShowLoans("Rejected loans", _services.FindClientsByRejectedLoans(), "There are no rejected loans.");
                            break;
                        case BanUser:
                            BanOrUnbanUser();
                            break;
                        case ViewLogs:
                            ShowLogs();
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (InvalidOperationException ex)
                {
                    ConsoleUi.Error(ex.Message);
                }
                ConsoleUi.Pause();
            }
        }

        private void ShowAllUsers()
        {
            List<ClientUser> clientUsers = _services.ViewAllUsers();
            if (clientUsers.Count == 0)
            {
                ConsoleUi.Info("There are no registered users.");
                return;
            }
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title("All users")
                .AddColumn("Name")
                .AddColumn("Email")
                .AddColumn("Accounts")
                .AddColumn(new TableColumn("Balance").RightAligned())
                .AddColumn("Loan")
                .AddColumn("Status");
            foreach (var user in clientUsers)
            {
                // One line per account, so account numbers line up with their balances.
                table.AddRow(
                    Markup.Escape(user.name),
                    EmailMarkup(user),
                    string.Join("\n", user.Accounts.Select(a => a.AccountNumber)),
                    string.Join("\n", user.Accounts.Select(a => ConsoleUi.Money(a.Balance))),
                    ConsoleUi.LoanStatusMarkup(user.Loan.Status),
                    BanStatusMarkup(user));
            }
            AnsiConsole.Write(table);
        }

        private void SearchForUser()
        {
            string nameToSearch = AnsiConsole.Prompt(new TextPrompt<string>("Name of the user to search:"));
            AnsiConsole.WriteLine();
            ClientUser foundUser = _services.FindClientUserByName(nameToSearch);
            if (foundUser == null)
            {
                ConsoleUi.Error("User not found.");
                return;
            }
            ShowUserDetails(foundUser);
        }

        // Shows one user and lets the admin act on them: ban or unban, and decide a pending loan.
        private void ShowUserDetails(ClientUser user)
        {
            var details = new Grid().AddColumns(2);
            details.AddRow("[grey]Status[/]", BanStatusMarkup(user));
            details.AddRow("[grey]Balance[/]", $"[bold]{ConsoleUi.Money(user.TotalBalance())}[/]");
            details.AddRow("[grey]Email[/]", EmailMarkup(user));
            details.AddRow("[grey]Monthly salary[/]", ConsoleUi.Money(user.Salary));
            details.AddRow("[grey]Loan[/]", ConsoleUi.LoanSummaryMarkup(user.Loan));
            AnsiConsole.Write(
                new Panel(details)
                    .Header(Markup.Escape(user.name))
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Grey));
            AnsiConsole.Write(ConsoleUi.AccountsTable(user.Accounts, "Accounts"));
            AnsiConsole.WriteLine();

            const string BanAction = "Ban this user";
            const string UnbanAction = "Unban this user";
            const string ApproveAction = "Approve loan";
            const string RejectAction = "Reject loan";
            const string BackAction = "Back";
            var actions = new List<string> { user.IsBanned ? UnbanAction : BanAction };
            if (user.Loan.Status == LoanStatus.Pending)
            {
                actions.Add(ApproveAction);
                actions.Add(RejectAction);
            }
            actions.Add(BackAction);
            string action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"What would you like to do with [bold]{Markup.Escape(user.name)}[/]?")
                    .AddChoices(actions));
            switch (action)
            {
                case BanAction:
                case UnbanAction:
                    ToggleBan(user);
                    break;
                case ApproveAction:
                    DecideLoan(user, approve: true);
                    break;
                case RejectAction:
                    DecideLoan(user, approve: false);
                    break;
                default:
                    throw new OperationCanceledException();
            }
        }

        private void ReviewLoanRequests()
        {
            List<ClientUser> requests = _services.FindClientsByRequestedLoans();
            if (requests.Count == 0)
            {
                ConsoleUi.Info("There are no requested loans.");
                return;
            }
            AnsiConsole.Write(LoansTable("Requested loans", requests, showSalary: true));
            AnsiConsole.WriteLine();

            ClientUser selectedUser = ConsoleUi.Select(
                "Choose a request to approve or reject:",
                requests,
                u => $"{Markup.Escape(u.name)}  [grey]{ConsoleUi.Money(u.Loan.RequestedAmount)}, {u.Loan.Time} months[/]");
            string decision = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Loan request from [bold]{Markup.Escape(selectedUser.name)}[/]:")
                    .AddChoices("Approve", "Reject", "Cancel"));
            if (decision == "Cancel")
            {
                throw new OperationCanceledException();
            }
            DecideLoan(selectedUser, approve: decision == "Approve");
        }

        // Approving or rejecting also emails the client, which can take a few seconds.
        private void DecideLoan(ClientUser user, bool approve)
        {
            if (approve)
            {
                string notification = AnsiConsole.Status().Start("Approving loan...", _ => _services.ApproveLoan(user));
                ConsoleUi.Success($"Loan approved for {user.name}.");
                ConsoleUi.Info(notification);
            }
            else
            {
                string notification = AnsiConsole.Status().Start("Rejecting loan...", _ => _services.RejectLoan(user));
                ConsoleUi.Success($"Loan rejected for {user.name}.");
                ConsoleUi.Info(notification);
            }
        }

        private void BanOrUnbanUser()
        {
            List<ClientUser> clientUsers = _services.ViewAllUsers();
            if (clientUsers.Count == 0)
            {
                ConsoleUi.Info("There are no registered users.");
                return;
            }
            ClientUser selectedUser = ConsoleUi.Select(
                "Choose a user:",
                clientUsers,
                u => $"{Markup.Escape(u.name)}  {BanStatusMarkup(u)}");
            ToggleBan(selectedUser);
        }

        // Banning or unbanning also emails the client, which can take a few seconds.
        private void ToggleBan(ClientUser user)
        {
            string name = Markup.Escape(user.name);
            if (!user.IsBanned)
            {
                if (!AnsiConsole.Confirm($"[red]Ban {name}?[/] They will not be able to log in until unbanned.", false))
                {
                    throw new OperationCanceledException();
                }
                string notification = AnsiConsole.Status().Start("Banning user...", _ => _services.BanClientUser(user));
                ConsoleUi.Success($"{user.name} has been banned.");
                ConsoleUi.Info(notification);
            }
            else
            {
                if (!AnsiConsole.Confirm($"{name} is banned. Unban them?", false))
                {
                    throw new OperationCanceledException();
                }
                string notification = AnsiConsole.Status().Start("Unbanning user...", _ => _services.UnbanClientUser(user));
                ConsoleUi.Success($"{user.name} has been unbanned.");
                ConsoleUi.Info(notification);
            }
        }

        private void ShowLogs()
        {
            List<string> logs = _services.ViewLogs();
            if (logs.Count == 0)
            {
                ConsoleUi.Info("The log is empty.");
                return;
            }
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title("Logs (newest first)")
                .AddColumn("Time")
                .AddColumn("Event");
            foreach (string line in logs)
            {
                // Entries are written as "[yyyy-MM-dd HH:mm:ss]: message".
                int separator = line.IndexOf("]: ", StringComparison.Ordinal);
                if (line.StartsWith('[') && separator > 0)
                {
                    table.AddRow(Markup.Escape(line[1..separator]), Markup.Escape(line[(separator + 3)..]));
                }
                else
                {
                    table.AddRow("[grey]-[/]", Markup.Escape(line));
                }
            }
            AnsiConsole.Write(table);
        }

        private static void ShowLoans(string title, List<ClientUser> users, string emptyMessage)
        {
            if (users.Count == 0)
            {
                ConsoleUi.Info(emptyMessage);
                return;
            }
            AnsiConsole.Write(LoansTable(title, users, showSalary: false));
        }

        private static Table LoansTable(string title, List<ClientUser> users, bool showSalary)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title(title)
                .AddColumn("User")
                .AddColumn(new TableColumn("Amount").RightAligned())
                .AddColumn(new TableColumn("Months").RightAligned())
                .AddColumn("To account");
            if (showSalary)
            {
                table.AddColumn(new TableColumn("Monthly salary").RightAligned());
            }
            foreach (var user in users)
            {
                var row = new List<string>
                {
                    Markup.Escape(user.name),
                    ConsoleUi.Money(user.Loan.RequestedAmount),
                    user.Loan.Time.ToString(),
                    user.Loan.Account,
                };
                if (showSalary)
                {
                    row.Add(ConsoleUi.Money(user.Salary));
                }
                table.AddRow(row.ToArray());
            }
            return table;
        }

        private static string BanStatusMarkup(ClientUser user)
        {
            return user.IsBanned ? "[red]Banned[/]" : "[green]Active[/]";
        }

        private static string EmailMarkup(ClientUser user)
        {
            return string.IsNullOrWhiteSpace(user.Email) ? "[grey]-[/]" : Markup.Escape(user.Email);
        }
    }
}
