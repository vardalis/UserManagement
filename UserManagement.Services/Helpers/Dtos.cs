using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Services.Helpers
{
    public class AuthenticateServiceResult
    {
        public string Token { get; set; }
        public string Role { get; set; }
    }
}
