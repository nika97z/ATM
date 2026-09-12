using ATM.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATM.Core.Models
{
    public class AdminUser : User
    {
        public AdminUser()
        {
        }

        public AdminUser(string name, string password, UserRole role) : base(name, password, role)
        {
        }
    }
}
