using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Entities;
using UserManagement.Services.Helpers;
using static UserManagement.Services.Helpers.RoleHelpers;

namespace UserManagement.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementService(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityRole> GetRoleByNameAsync(string name)
        {
            return await _context.Roles.AsNoTracking().Where(role => role.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetAllUsersCountAsync(string searchString)
        {
            var users = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(searchString))
                users = users.Where(user => (user.LastName.Contains(searchString)
                    || user.FirstName.Contains(searchString)
                    || user.Email.Contains(searchString)));

            return await users.CountAsync();
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync(string searchString)
        {
            var users = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(searchString))
                users = users.Where(user => (user.LastName.Contains(searchString)
                    || user.FirstName.Contains(searchString)
                    || user.Email.Contains(searchString)));

            return await users.ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetUsersAsync(int offset, int limit, string sortOrder, string searchString)
        {
            offset = offset < 0 ? 0 : offset;
            limit = limit < 0 ? 0 : limit;


            var pageUsers = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(searchString))
                pageUsers = pageUsers.Where(user => (user.LastName.Contains(searchString)
                    || user.FirstName.Contains(searchString)
                    || user.Email.Contains(searchString)));

            switch (sortOrder)
            {
                case "Lname":
                    pageUsers = pageUsers.OrderBy(u => u.LastName);
                    break;
                case "Lname_desc":
                    pageUsers = pageUsers.OrderByDescending(u => u.LastName);
                    break;
                case "Fname":
                    pageUsers = pageUsers.OrderBy(u => u.FirstName);
                    break;
                case "Fname_desc":
                    pageUsers = pageUsers.OrderByDescending(u => u.FirstName);
                    break;
                case "Email":
                    pageUsers = pageUsers.OrderBy(u => u.Email);
                    break;
                case "Email_desc":
                    pageUsers = pageUsers.OrderByDescending(u => u.Email);
                    break;
                case "Approved":
                    pageUsers = pageUsers.OrderBy(u => u.Approved);
                    break;
                case "Approved_desc":
                    pageUsers = pageUsers.OrderByDescending(u => u.Approved);
                    break;
                default:
                    pageUsers = pageUsers.OrderBy(u => u.LastName);
                    break;
            }

            pageUsers = pageUsers.Skip(offset).Take(limit);

            return await pageUsers.ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetInternalUsersAsync()
        {
            var supervisors = await _userManager.GetUsersInRoleAsync("supervisor");
            var employees = await _userManager.GetUsersInRoleAsync("employee");
            return supervisors.Concat(employees).ToList();
        }

        public async Task<string> GetUserRoleAsync(string userId, bool returnName)
        {
            ApplicationUser user = await _context.Users.AsNoTracking().Where(u => u.Id == userId).FirstOrDefaultAsync();
            var roles = await _userManager.GetRolesAsync(user);

            foreach (RolePair rolePair in RoleHelpers.Roles)
            {
                IdentityRole identityRole = await _context.Roles.AsNoTracking().Where(role => role.Name == rolePair.Name)
                    .FirstOrDefaultAsync();
                if (roles.Contains(identityRole.Name))
                    return returnName ? rolePair.Name : rolePair.Description;
            }
            return "";
        }

        public async Task<ApplicationUser> FindUserAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }

        public async Task<ApplicationUser> FindUserAsync(ClaimsPrincipal claimsPrincipal)
        {
            ApplicationUser user = await _userManager.GetUserAsync(claimsPrincipal);
            return await FindUserAsync(user.Id);
        }

        public async Task<string> FindUserIdAsync(ClaimsPrincipal claimsPrincipal)
        {
            ApplicationUser user = await _userManager.GetUserAsync(claimsPrincipal);
            return user.Id;
        }

        public async Task<ApplicationUser> FindInternalUserAsync(string userId)
        {
            var user = await _userManager.Users.AsNoTracking().SingleAsync(u => u.Id == userId);

            string role = await GetUserRoleAsync(user.Id, true);
            if (role == "supervisor" || role == "employee")
                return user;

            return null;
        }

        public async Task<IdentityResult> AddUserAsync(ApplicationUser user, string password, string role)
        {
            // In case of racing conditions the error will be returned by the CreateAsync method
            // This extra check is made becuase CreateAsync produces errors for both email and username
            // (when only the email is exposed to the GUI)
            if (await _userManager.FindByEmailAsync(user.Email) != null)
                return IdentityResult.Failed(new IdentityError() { Description = "Email already in use!" });

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, role);

            return result;
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user, string newUserRole, byte[] rowVersion)
        {
            // _context.Entry(departmentToUpdate).Property("RowVersion").OriginalValue = rowVersion;
            _context.Entry(user).State = EntityState.Modified;
            _context.Entry(user).Property("RowVersion").OriginalValue = rowVersion;

            // var result = await _userManager.UpdateAsync(user);
            // var result = _context.Users.Update(user);

            await _context.SaveChangesAsync();

            string[] existingRoles = (await _userManager.GetRolesAsync(user)).ToArray();
            var result = await _userManager.RemoveFromRolesAsync(user, existingRoles);

            if (result.Succeeded)
                result = await _userManager.AddToRoleAsync(user, newUserRole);

            return result;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
                result = await _userManager.AddPasswordAsync(user, password);

            return result;
        }

        public async Task DisallowApplicationEditingAsync(string userId)
        {
            var user = await FindUserAsync(userId);
            user.ApplicationEditingAllowed = false;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsEmailInUseAsync(string email)
        {
            return await IsEmailInUseAsync(email, null);
        }

        public async Task<bool> IsEmailInUseAsync(string email, string excludeUserID)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            return user != null && user.Id != excludeUserID;
        }

        // Check necessity/operation of the functions below

        /*
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
        */

        // Consider for future use when operating directly on the context vs. the user manager

        /*
        try
        {
            // _context.Update(applicationUser);
            // await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ApplicationUserExists(applicationUser.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        */
    }
}
