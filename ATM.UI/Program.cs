using ATM.Core.Exceptions;
using ATM.Core.Interfaces;
using ATM.Core.Models;
using ATM.Infrastructure.Repository;
using ATM.Services;
using ATM.UI;
using Spectre.Console;
using System.Text;

internal class Program
{
    private const string RegisterUser = "Register user";
    private const string RegisterAdmin = "Register admin";
    private const string LoginUser = "Log in as user";
    private const string LoginAdmin = "Log in as admin";
    private const string Exit = "Exit";

    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        Interface1 repository = new Repository();

        IEmailService emailService = new EmailService();

        ATMServices services = new ATMServices(repository, emailService);

        while (true)
        {
            ConsoleUi.ShowBanner();
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an option:")
                    .AddChoices(RegisterUser, RegisterAdmin, LoginUser, LoginAdmin, Exit));
            try
            {
                switch (choice)
                {
                    case RegisterUser:
                        ClientUser registeredUser = RegisterClient(services);
                        ConsoleUi.Pause();
                        new UserManu(services).Show(registeredUser);
                        continue;
                    case RegisterAdmin:
                        AdminUser registeredAdmin = RegisterAdministrator(services);
                        ConsoleUi.Pause();
                        new AdminManu(services).Show(registeredAdmin);
                        continue;
                    case LoginUser:
                        ConsoleUi.ShowTitle("User login");
                        ClientUser loggedInUser = services.LoginUser(PromptName(), ConsoleUi.PromptPassword());
                        new UserManu(services).Show(loggedInUser);
                        continue;
                    case LoginAdmin:
                        ConsoleUi.ShowTitle("Admin login");
                        User loggedInAdmin = services.LoginAdmin(PromptName(), ConsoleUi.PromptPassword());
                        new AdminManu(services).Show(loggedInAdmin);
                        continue;
                    case Exit:
                        return;
                }
            }
            catch (UserBannedException ex)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(
                    new Panel(new Markup($"[bold red]{Markup.Escape(ex.Message)}[/]"))
                        .Header("[red]Banned[/]")
                        .Border(BoxBorder.Heavy)
                        .BorderColor(Color.Red));
            }
            catch (Exception ex)
            {
                ConsoleUi.Error(ex.Message);
            }
            ConsoleUi.Pause();
        }
    }

    private static string PromptName()
    {
        return AnsiConsole.Prompt(new TextPrompt<string>("Name:"));
    }

    private static string PromptNewName(Func<string, bool> isTaken)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Name:")
                .Validate(n =>
                {
                    if (!ATMServices.IsValidName(n))
                    {
                        return ValidationResult.Error($"[red]{ATMServices.NameRule}[/]");
                    }
                    if (isTaken(n))
                    {
                        return ValidationResult.Error("[red]A user with that name already exists. Please choose a different name.[/]");
                    }
                    return ValidationResult.Success();
                }));
    }

    private static ClientUser RegisterClient(ATMServices services)
    {
        ConsoleUi.ShowTitle("Register user");
        string name = PromptNewName(services.IsClientNameTaken);
        string password = ConsoleUi.PromptNewPassword();
        decimal salary = AnsiConsole.Prompt(
            new TextPrompt<decimal>("Monthly salary (gel):")
                .Validate(s => s >= 0, $"[red]{ATMServices.SalaryRule}[/]"));
        string email = AnsiConsole.Prompt(
            new TextPrompt<string>("Email:")
                .Validate(e =>
                {
                    if (!ATMServices.IsValidEmail(e.Trim()))
                    {
                        return ValidationResult.Error("[red]Invalid email address.[/]");
                    }
                    if (services.IsEmailTaken(e.Trim()))
                    {
                        return ValidationResult.Error("[red]A user with that email already exists. Please use a different email.[/]");
                    }
                    return ValidationResult.Success();
                })).Trim();

        AnsiConsole.Status().Start(
            "Sending verification code...",
            _ => services.StartUserRegistration(name, password, salary, email));
        ConsoleUi.Info($"A 4-digit verification code has been sent to {email}.");

        ClientUser? clientUser = null;
        while (clientUser == null)
        {
            string code = AnsiConsole.Prompt(new TextPrompt<string>("Verification code:")).Trim();
            clientUser = services.CompleteUserRegistration(code);
            if (clientUser == null)
            {
                ConsoleUi.Error("Incorrect code. Please try again.");
            }
        }
        ConsoleUi.Success($"Email verified. Registration successful! Your account number is {clientUser.Accounts[0].AccountNumber}.");
        return clientUser;
    }

    private static AdminUser RegisterAdministrator(ATMServices services)
    {
        ConsoleUi.ShowTitle("Register admin");
        string name = PromptNewName(services.IsAdminNameTaken);
        string password = ConsoleUi.PromptNewPassword();
        AdminUser adminUser = services.RegisterAdminUser(name, password);
        ConsoleUi.Success("Admin registration successful!");
        return adminUser;
    }
}
