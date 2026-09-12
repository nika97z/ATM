using ATM.Core.Enums;

namespace ATM.Core.Models
{
    public class User
    {
        public int id { get; set; }

        public User(string name, string password, UserRole role)
        {
            this.name = name;
            Password = password;
            Role = role;
        }

        public User()
        {
        }

        public string name { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
    }
}
