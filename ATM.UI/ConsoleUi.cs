using ATM.Core.Enums;
using ATM.Core.Models;
using ATM.Services;
using Spectre.Console;

namespace ATM.UI
{

    internal static class ConsoleUi
    {
        public const string Accent = "deepskyblue1";

        public static void ShowBanner()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("ATM").Centered().Color(Color.DeepSkyBlue1));
            AnsiConsole.Write(new Rule("[grey]ATM Bank[/]").RuleStyle(new Style(Color.Grey)));
            AnsiConsole.WriteLine();
        }

        public static void ShowTitle(string title)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold {Accent}]{Markup.Escape(title)}[/]").LeftJustified().RuleStyle(new Style(Color.Grey)));
            AnsiConsole.WriteLine();
        }

        public static void Success(string message)
        {
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(message)}[/]");
        }

        public static void Error(string message)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");
        }

        public static void Info(string message)
        {
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(message)}[/]");
        }

        public static void Pause()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        public static string Money(decimal amount)
        {
            return $"{amount:N2} gel";
        }

        public static string PromptPassword()
        {
            return AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());
        }

        public static string PromptNewPassword()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Password [grey](at least 8 characters, including a number)[/]:")
                    .Secret()
                    .Validate(ATMServices.IsValidPassword, $"[red]{ATMServices.PasswordRule}[/]"));
        }


        public static decimal PromptAmount(string title, decimal greaterThan = 0m, decimal max = decimal.MaxValue)
        {
            decimal amount = AnsiConsole.Prompt(
                new TextPrompt<decimal>($"{title} [grey](0 to cancel)[/]:")
                    .Validate(value =>
                    {
                        if (value == 0)
                        {
                            return ValidationResult.Success();
                        }
                        if (value <= greaterThan)
                        {
                            return ValidationResult.Error($"[red]Amount must be greater than {Money(greaterThan)}.[/]");
                        }
                        if (value > max)
                        {
                            return ValidationResult.Error($"[red]Amount cannot be more than {Money(max)}.[/]");
                        }
                        return ValidationResult.Success();
                    }));
            if (amount == 0)
            {
                throw new OperationCanceledException();
            }
            return amount;
        }

        public static T Select<T>(string title, IReadOnlyList<T> items, Func<T, string> describe)
        {
            const int cancel = -1;
            int index = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title(title)
                    .PageSize(10)
                    .AddChoices(Enumerable.Range(0, items.Count))
                    .AddChoices(cancel)
                    .UseConverter(i => i == cancel ? "[grey]Cancel[/]" : describe(items[i])));
            if (index == cancel)
            {
                throw new OperationCanceledException();
            }
            return items[index];
        }

        public static Account SelectAccount(string title, IReadOnlyList<Account> accounts, bool showBalance = true)
        {
            if (accounts.Count == 0)
            {
                throw new InvalidOperationException("There are no accounts to choose from.");
            }
            if (accounts.Count == 1)
            {
                return accounts[0];
            }
            return Select(title, accounts, a => showBalance
                ? $"{a.AccountNumber}  [grey]{Money(a.Balance)}[/]"
                : a.AccountNumber);
        }

        public static Table AccountsTable(IEnumerable<Account> accounts, string title)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title(title)
                .AddColumn("Account number")
                .AddColumn(new TableColumn("Balance").RightAligned());
            foreach (var account in accounts)
            {
                table.AddRow(account.AccountNumber, Money(account.Balance));
            }
            return table;
        }

        public static string LoanStatusMarkup(LoanStatus status)
        {
            return status switch
            {
                LoanStatus.Pending => "[yellow]Pending[/]",
                LoanStatus.Approved => "[green]Approved[/]",
                LoanStatus.Rejected => "[red]Rejected[/]",
                _ => "[grey]None[/]",
            };
        }

        public static string LoanSummaryMarkup(Loans loan)
        {
            if (loan.Status == LoanStatus.DidnotRequested)
            {
                return "[grey]No loan requested.[/]";
            }
            return $"{LoanStatusMarkup(loan.Status)}  {Money(loan.RequestedAmount)} for {loan.Time} months to account {loan.Account}";
        }
    }
}
