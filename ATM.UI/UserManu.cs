using ATM.Core.Enums;
using ATM.Core.Exceptions;
using ATM.Core.Models;
using ATM.Services;
using Spectre.Console;

namespace ATM.UI
{
    internal class UserManu
    {
        private const decimal MaxAmount = 10000000000m;

        private const string OpenAccount = "Open a new account";
        private const string DeleteAccount = "Delete an account";
        private const string Deposit = "Deposit money";
        private const string Withdraw = "Withdraw money";
        private const string TransferOwn = "Transfer between my accounts";
        private const string TransferByName = "Transfer to another user by name";
        private const string TransferByAccount = "Transfer to another user by account number";
        private const string RequestLoan = "Request a loan";
        private const string UpdateSalary = "Update monthly salary";
        private const string DeleteProfile = "Delete my profile";
        private const string LogOut = "Log out";

        private readonly ATMServices _services;

        public UserManu(ATMServices services)
        {
            _services = services;
        }

        public void Show(ClientUser clientUser)
        {
            while (true)
            {
                ShowDashboard(clientUser);
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What would you like to do?")
                        .PageSize(12)
                        .AddChoices(OpenAccount, DeleteAccount, Deposit, Withdraw, TransferOwn, TransferByName, TransferByAccount, RequestLoan, UpdateSalary, DeleteProfile, LogOut));
                if (choice == LogOut)
                {
                    return;
                }

                try
                {
                    switch (choice)
                    {
                        case OpenAccount:
                            _services.AddAccountToClientUser(clientUser);
                            ConsoleUi.Success($"New account {clientUser.Accounts[^1].AccountNumber} opened.");
                            break;
                        case DeleteAccount:
                            DeleteEmptyAccount(clientUser);
                            break;
                        case Deposit:
                            DepositMoney(clientUser);
                            break;
                        case Withdraw:
                            WithdrawMoney(clientUser);
                            break;
                        case TransferOwn:
                            TransferBetweenOwnAccounts(clientUser);
                            break;
                        case TransferByName:
                            TransferToUserByName(clientUser);
                            break;
                        case TransferByAccount:
                            TransferToAccountNumber(clientUser);
                            break;
                        case RequestLoan:
                            RequestNewLoan(clientUser);
                            break;
                        case UpdateSalary:
                            UpdateMonthlySalary(clientUser);
                            break;
                        case DeleteProfile:
                            if (!AnsiConsole.Confirm("[red]Delete your profile and all of its accounts? This cannot be undone.[/]", false))
                            {
                                continue;
                            }
                            _services.DeleteClientUser(clientUser);
                            ConsoleUi.Success("Your profile has been deleted.");
                            ConsoleUi.Pause();
                            return;
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (Exception ex) when (ex is InsufficientFundsException || ex is LoanExeption || ex is DeleteAccountExtension || ex is InvalidOperationException)
                {
                    ConsoleUi.Error(ex.Message);
                }
                ConsoleUi.Pause();
            }
        }

        private static void ShowDashboard(ClientUser clientUser)
        {
            ConsoleUi.ShowTitle($"Welcome, {clientUser.name}!");
            var summary = new Grid().AddColumns(2);
            summary.AddRow("Total balance:", $"[bold green]{ConsoleUi.Money(clientUser.TotalBalance())}[/]");
            summary.AddRow("[grey]Monthly salary:[/]", $"[grey]{ConsoleUi.Money(clientUser.Salary)}[/]");
            AnsiConsole.Write(summary);
            AnsiConsole.WriteLine();
            if (clientUser.Accounts.Count == 0)
            {
                ConsoleUi.Info("You have no accounts. Open one from the menu below.");
            }
            else
            {
                AnsiConsole.Write(ConsoleUi.AccountsTable(clientUser.Accounts, "Your accounts"));
            }
            AnsiConsole.Write(
                new Panel(new Markup(ConsoleUi.LoanSummaryMarkup(clientUser.Loan)))
                    .Header("Your loan")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Grey));
            AnsiConsole.WriteLine();
        }

        private void DeleteEmptyAccount(ClientUser clientUser)
        {
            if (clientUser.Accounts.Count <= 1)
            {
                ConsoleUi.Error("You cannot delete your only account.");
                return;
            }
            List<Account> emptyAccounts = clientUser.Accounts.Where(a => a.Balance == 0).ToList();
            if (emptyAccounts.Count == 0)
            {
                ConsoleUi.Error("Only an account with no money in it can be deleted. Withdraw or transfer the money first.");
                return;
            }
            Account account = ConsoleUi.SelectAccount("Delete which account? [grey](only empty accounts are listed)[/]", emptyAccounts);
            if (!AnsiConsole.Confirm($"Delete account {account.AccountNumber}?", false))
            {
                throw new OperationCanceledException();
            }
            _services.DeleteAccountFromClientUser(clientUser, account.AccountNumber);
            ConsoleUi.Success($"Account {account.AccountNumber} deleted.");
        }

        private void DepositMoney(ClientUser clientUser)
        {
            Account account = ConsoleUi.SelectAccount("Deposit to which account?", clientUser.Accounts);
            decimal amount = ConsoleUi.PromptAmount("Amount to deposit", max: MaxAmount);
            _services.AddMoneyToAccount(clientUser, account.AccountNumber, amount);
            ConsoleUi.Success($"Deposited {ConsoleUi.Money(amount)} to account {account.AccountNumber}.");
        }

        private void WithdrawMoney(ClientUser clientUser)
        {
            Account account = ConsoleUi.SelectAccount("Withdraw from which account?", clientUser.Accounts);
            decimal amount = ConsoleUi.PromptAmount("Amount to withdraw");
            _services.SubtractMoneyFromAccount(clientUser, account.AccountNumber, amount);
            ConsoleUi.Success($"Withdrew {ConsoleUi.Money(amount)} from account {account.AccountNumber}.");
        }

        // Checked before asking for any transfer details, so the user is told straight away.
        private static bool HasMoneyToTransfer(ClientUser clientUser)
        {
            if (clientUser.TotalBalance() > 0)
            {
                return true;
            }
            ConsoleUi.Error($"You cannot transfer money because your total balance is {ConsoleUi.Money(0)}.");
            return false;
        }

        private void TransferBetweenOwnAccounts(ClientUser clientUser)
        {
            if (!HasMoneyToTransfer(clientUser))
            {
                return;
            }
            if (clientUser.Accounts.Count < 2)
            {
                ConsoleUi.Error("You have only one account. Open another account to transfer between them.");
                return;
            }
            Account from = ConsoleUi.SelectAccount("Transfer from which account?", clientUser.Accounts);
            Account to = ConsoleUi.SelectAccount("Transfer to which account?", clientUser.Accounts.Where(a => a != from).ToList());
            decimal amount = ConsoleUi.PromptAmount("Amount to transfer");
            _services.TransferMoneyToOwnerAccount(clientUser, from.AccountNumber, to.AccountNumber, amount);
            ConsoleUi.Success($"Transferred {ConsoleUi.Money(amount)} from account {from.AccountNumber} to account {to.AccountNumber}.");
        }

        private void TransferToUserByName(ClientUser clientUser)
        {
            if (!HasMoneyToTransfer(clientUser))
            {
                return;
            }
            string receiverName = AnsiConsole.Prompt(new TextPrompt<string>("Recipient's name:"));
            var receiver = _services.FindClientUserByName(receiverName);
            if (receiver == null)
            {
                ConsoleUi.Error("User not found.");
                return;
            }
            if (receiver.id == clientUser.id)
            {
                ConsoleUi.Error($"You cannot transfer money to yourself this way. Use \"{TransferOwn}\" instead.");
                return;
            }
            if (receiver.Accounts.Count == 0)
            {
                ConsoleUi.Error("The recipient has no accounts. Cannot transfer money.");
                return;
            }
            Account from = ConsoleUi.SelectAccount("Transfer from which account?", clientUser.Accounts);
            Account to = ConsoleUi.SelectAccount($"Transfer to which of {Markup.Escape(receiver.name)}'s accounts?", receiver.Accounts, showBalance: false);
            decimal amount = ConsoleUi.PromptAmount("Amount to transfer");
            _services.TransferMoneyToAnotherUser(clientUser, receiver, from.AccountNumber, to.AccountNumber, amount);
            ConsoleUi.Success($"Transferred {ConsoleUi.Money(amount)} to {receiver.name} (account {to.AccountNumber}).");
        }

        private void TransferToAccountNumber(ClientUser clientUser)
        {
            if (!HasMoneyToTransfer(clientUser))
            {
                return;
            }
            // Lowercase input such as ge123456789 is accepted and turned into GE123456789.
            string receiverAccountNumber = AnsiConsole.Prompt(
                new TextPrompt<string>("Recipient's account number [grey](for example GE123456789)[/]:")
                    .Validate(n => ATMServices.IsValidAccountNumber(n.Trim().ToUpperInvariant()), $"[red]{ATMServices.AccountNumberRule}[/]"))
                .Trim().ToUpperInvariant();
            var receiver = _services.FindClientUserByAccountNumber(receiverAccountNumber);
            if (receiver == null)
            {
                ConsoleUi.Error("User with that account number not found.");
                return;
            }
            if (receiver.id == clientUser.id)
            {
                ConsoleUi.Error($"You cannot transfer money to yourself this way. Use \"{TransferOwn}\" instead.");
                return;
            }
            Account from = ConsoleUi.SelectAccount("Transfer from which account?", clientUser.Accounts);
            decimal amount = ConsoleUi.PromptAmount("Amount to transfer");
            _services.TransferMoneyToAnotherUser(clientUser, receiver, from.AccountNumber, receiverAccountNumber, amount);
            ConsoleUi.Success($"Transferred {ConsoleUi.Money(amount)} to account {receiverAccountNumber}.");
        }

        private void RequestNewLoan(ClientUser clientUser)
        {
            string? blockedReason = ATMServices.LoanRequestBlockedReason(clientUser);
            if (blockedReason != null)
            {
                ConsoleUi.Error(blockedReason);
                return;
            }
            decimal amount = ConsoleUi.PromptAmount("Loan amount", greaterThan: 400m, max: MaxAmount);
            int months = AnsiConsole.Prompt(
                new TextPrompt<int>("Loan term in months [grey](5-48)[/]:")
                    .Validate(m => m >= 5 && m <= 48, "[red]Loan time must be between 5 and 48 months.[/]"));
            Account account = ConsoleUi.SelectAccount("Which account should receive the loan?", clientUser.Accounts);
            _services.RequestLoan(clientUser, amount, months, account.AccountNumber);
            ConsoleUi.Success("Loan request submitted. An admin will review it.");
        }

        private void UpdateMonthlySalary(ClientUser clientUser)
        {
            ConsoleUi.Info($"Current monthly salary: {ConsoleUi.Money(clientUser.Salary)}");
            decimal newSalary = AnsiConsole.Prompt(
                new TextPrompt<decimal>("New monthly salary (gel):")
                    .Validate(s => s >= 0, $"[red]{ATMServices.SalaryRule}[/]"));
            _services.UpdateSalary(clientUser, newSalary);
            ConsoleUi.Success($"Monthly salary updated to {ConsoleUi.Money(newSalary)}.");
            if (clientUser.Loan.Status == LoanStatus.Approved || clientUser.Loan.Status == LoanStatus.Rejected)
            {
                ConsoleUi.Info("You can now request a new loan.");
            }
        }
    }
}
