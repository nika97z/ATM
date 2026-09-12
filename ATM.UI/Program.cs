using ATM.Core.Enums;
using ATM.Core.Interfaces;
using ATM.Core.Models;
using ATM.Infrastructure.Repository;
using ATM.Services;
using System.Data;

internal class Program
{
    private static void Main(string[] args)
    {

        Interface1 repository = new Repository();

        Services services = new Services(repository);

        Console.WriteLine("Choose an option:");
        Console.WriteLine("1. Register User");
        Console.WriteLine("2. Register Admin");
        Console.WriteLine("3. Login to User");
        Console.WriteLine("4. Login to Admin");
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
                    services.LoginUser();
                    break;
                case "4":
                    services.LoginAdmin();
                    break;
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
