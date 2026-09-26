using ATM.Core.Interfaces;
using ATM.Core.Models;
using ATM.Infrastructure.Repository;
using ATM.Services;
using ATM.UI;

internal class Program
{
    private static void Main(string[] args)
    {

        Interface1 repository = new Repository();

        ATMServices services = new ATMServices(repository);

        while (true)
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Register User");
            Console.WriteLine("2. Register Admin");
            Console.WriteLine("3. Login to User");
            Console.WriteLine("4. Login to Admin");
            Console.WriteLine("5. Exit");
            string choice = Console.ReadLine();
            try
            {
                switch (choice)
                {
                    case "1":
                        ClientUser clientUser = new ClientUser();
                        services.RegisterUser(clientUser);
                        Console.WriteLine("Registration successful!");
                        break;
                    case "2":
                        AdminUser adminUser = new AdminUser();
                        services.RegisterAdminUser(adminUser);
                        Console.WriteLine("Admin registration successful!");
                        break;
                    case "3":
                        ClientUser loggedInUser = services.LoginUser();
                        var userMenu = new UserManu(services, repository);
                        userMenu.Show(services, loggedInUser);
                        break;
                    case "4":
                        User loggedInAdmin = services.LoginAdmin();
                        var adminMenu = new AdminManu(services, repository);
                        adminMenu.Show(services, loggedInAdmin);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
