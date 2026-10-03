using ATM.Core.Models;

namespace ATM.Core.Interfaces
{
    public interface Interface1
    {
        void RegisterClientUser(ClientUser clientUser);

        List<User> GetAll();
        List<ClientUser> GetAllClientUsers();
        List<AdminUser> GetAllAdminUsers();

        void RegisterAdminUser(AdminUser adminUser);
        void UpdateClientUser(ClientUser updatedClientUser);
        void DeleteClientUser(ClientUser clientUser);
        void Log(string message);

    }
}
