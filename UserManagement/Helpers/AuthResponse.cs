using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Helpers
{
    public class AuthResponse
    {
        public bool success;
        public string token;
        public int expiresInMinutes;
        public string message;
        public string email;
        public string role;
    }
}
