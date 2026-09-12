using ATM.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
