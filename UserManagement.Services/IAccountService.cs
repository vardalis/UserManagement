using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Entities;
using UserManagement.Services.Helpers;

namespace UserManagement.Services
{
    public interface IAccountService
    {
        Task<AuthenticateServiceResult> AuthenticateAsync(string username, string password, int expiresInMinutes,
            string validIssuer, string validAudience, SecurityKey symmetricSecurityKey);
        Task<string> GetUserRoleAsync(string email);
    }
}
