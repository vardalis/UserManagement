using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Services.Helpers
{
    public static class RoleHelpers
    {
        public struct RolePair
        {
            public string Name { get; set; }
            public string Description { get; set; }
        };

        public static List<RolePair> Roles = new List<RolePair>()
        {
            new RolePair { Name = "admin", Description = "Administrator"},
            new RolePair { Name = "supervisor", Description = "Supervisor" },
            new RolePair { Name = "employee", Description =  "Employee"},
            new RolePair { Name = "applicant", Description = "Applicant"}
        };
    }
}
