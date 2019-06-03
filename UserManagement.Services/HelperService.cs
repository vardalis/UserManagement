using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using UserManagement.Entities;

namespace UserManagement.Services
{
    public class HelperService : IHelperService
    {
        public HelperService()
        {
        }

        public async Task<PropertyValues> RetrieveEntity(DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var clientUser = (ApplicationUser)entry.Entity;
            return await entry.GetDatabaseValuesAsync();
        }
    }
}
