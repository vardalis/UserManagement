using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagement.Entities;
using UserManagement.Services.Helpers;
using static UserManagement.Services.Helpers.RoleHelpers;

namespace UserManagement.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountService(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<AuthenticateServiceResult> AuthenticateAsync(string username, string password, int expiresInMinutes, string validIssuer, 
            string validAudience, SecurityKey symmetricSecurityKey)
        {
            ApplicationUser user = await _userManager.Users
                .Where(u => u.UserName == username).FirstOrDefaultAsync();

            // return null if user not found, email not confirmed or password incorrect
            if (user == null || !user.EmailConfirmed || !user.Approved ||
                ! await _userManager.CheckPasswordAsync(user, password) )
                return null;

            string role = await GetUserRoleAsync(username);

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
                Issuer = validIssuer,
                Audience = validAudience,
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            AuthenticateServiceResult result = new AuthenticateServiceResult
            {
                Role = role,
                Token = tokenHandler.WriteToken(token)
            };

            return result;
        }

        public async Task<string> GetUserRoleAsync(string email)
        {
            ApplicationUser user = await _context.Users.AsNoTracking().Where(u => u.Email == email).FirstOrDefaultAsync();
            var roles = await _userManager.GetRolesAsync(user);

            foreach (RolePair rolePair in RoleHelpers.Roles)
            {
                IdentityRole identityRole = await _context.Roles.AsNoTracking().Where(role => role.Name == rolePair.Name).FirstOrDefaultAsync();
                if (roles.Contains(identityRole.Name))
                    return rolePair.Name;
            }
            return "";
        }
    }
}
