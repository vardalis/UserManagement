using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Entities;

namespace UserManagement.Services
{
    public interface IUserManagementService
    {
        Task<IdentityRole> GetRoleByNameAsync(string name);
        Task<int> GetAllUsersCountAsync(string searchString);
        Task<List<ApplicationUser>> GetAllUsersAsync(string searchString);
        // Task<PaginatedList<ApplicationUser>> GetAllUsersPaginatedAsync(int pageIndex, int pageSize, string searchString, string sortOrder);
        Task<List<ApplicationUser>> GetUsersAsync(int offset, int limit, string sortOrder, string searchString);
        Task<List<ApplicationUser>> GetInternalUsersAsync();
        Task<string> GetUserRoleAsync(string userId, bool returnName);
        Task<ApplicationUser> FindUserAsync(string userId);
        Task<ApplicationUser> FindUserAsync(ClaimsPrincipal claimsPrincipal);
        Task<string> FindUserIdAsync(ClaimsPrincipal claimsPrincipal);
        Task<ApplicationUser> FindInternalUserAsync(string userId);
        Task<IdentityResult> AddUserAsync(ApplicationUser user, string password, string role);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user, string newUserRole, byte[] rowVersion);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string password);
        Task DisallowApplicationEditingAsync(string userId);
        Task<bool> IsEmailInUseAsync(string email);
        Task<bool> IsEmailInUseAsync(string email, string excludeUserID);
    }
}
