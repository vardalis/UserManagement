using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Services
{
    public interface IHelperService
    {
        Task<PropertyValues> RetrieveEntity(DbUpdateConcurrencyException ex);
    }
}
