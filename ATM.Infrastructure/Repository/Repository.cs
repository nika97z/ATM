using ATM.Core.Enums;
using ATM.Core.Interfaces;
using ATM.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ATM.Infrastructure.Repository
{
    public class Repository : Interface1
    {
        string _path = "C:\\Users\\User\\OneDrive\\Desktop\\ATM\\ATM.Infrastructure\\Data\\Data.txt";

        private readonly string _logFilePath = "C:\\Users\\User\\OneDrive\\Desktop\\ATM\\ATM.Infrastructure\\Data\\Log.txt";

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public void Log(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: { message}";
            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
        }


        public List<User> GetAll()
        {
            var AllUsers = new List<User>();
            var lines = File.ReadAllLines(_path);
            foreach(var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var clientUser = JsonSerializer.Deserialize<ClientUser>(line, _jsonOptions);
                if (clientUser != null)
                {
                    AllUsers.Add(clientUser);
                }
            }
            return AllUsers;
        }
        public List<AdminUser> GetAllAdminUsers()
        {
            var adminUsers = new List<AdminUser>();
            var lines = File.ReadAllLines(_path);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var adminUser = JsonSerializer.Deserialize<AdminUser>(line, _jsonOptions);
                if (adminUser != null && adminUser.Role == UserRole.Admin)
                {
                    adminUsers.Add(adminUser);
                }
            }
            return adminUsers;
        }

        public List<ClientUser> GetAllClientUsers()
        {
            var clientUsers = new List<ClientUser>();
            var lines = File.ReadAllLines(_path);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var clientUser = JsonSerializer.Deserialize<ClientUser>(line, _jsonOptions);
                if (clientUser != null && clientUser.Role == UserRole.User)
                {
                    clientUsers.Add(clientUser);
                }
            }
            return clientUsers;
        }

        public void RegisterClientUser(ClientUser clientUser)
        {
            var json = JsonSerializer.Serialize(clientUser);
            using (StreamWriter writer = new StreamWriter(_path, true)) { writer.WriteLine(json); }
        }

        public void RegisterAdminUser(AdminUser adminUser)
        {
            var json = JsonSerializer.Serialize(adminUser);
            using (StreamWriter writer = new StreamWriter(_path, true)) { writer.WriteLine(json); }
        }

        public void UpdateClientUser(ClientUser updatedClientUser)
        {
            var allUsers = GetAllClientUsers();
            var updatedUsers = new List<ClientUser>();
            foreach (var user in allUsers)
            {
                if (user.id == updatedClientUser.id)
                {
                    updatedUsers.Add(updatedClientUser);
                }
                else
                {
                    updatedUsers.Add(user);
                }
            }
            var adminUsers = GetAllAdminUsers();
            using (StreamWriter writer = new StreamWriter(_path, false))
            {
                foreach (var admin in adminUsers)
                {
                    var json = JsonSerializer.Serialize(admin);
                    writer.WriteLine(json);
                }
                foreach (var user in updatedUsers)
                {
                    var json = JsonSerializer.Serialize(user);
                    writer.WriteLine(json);
                }
            }
        }
        public void DeleteClientUser(ClientUser clientUser)
        {
            var allUsers = GetAllClientUsers();
            var updatedUsers = new List<ClientUser>();
            foreach (var user in allUsers)
            {
                if (user.id != clientUser.id)
                {
                    updatedUsers.Add(user);
                }
            }
            var adminUsers = GetAllAdminUsers();
            using (StreamWriter writer = new StreamWriter(_path, false))
            {
                foreach (var admin in adminUsers)
                {
                    var json = JsonSerializer.Serialize(admin);
                    writer.WriteLine(json);
                }
                foreach (var user in updatedUsers)
                {
                    var json = JsonSerializer.Serialize(user);
                    writer.WriteLine(json);
                }
            }
        }


    }
}
