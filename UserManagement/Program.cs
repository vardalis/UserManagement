using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserManagement.Entities;
using UserManagement.Extensions;
using UserManagement.Services;
using UserManagement.Services.Helpers;
using static UserManagement.Services.Helpers.RoleHelpers;

namespace UserManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostBuilder = CreateWebHostBuilder(args);
            var host = hostBuilder.Build();

            // TODO: uncomment below for database initialization
            InitializeDatabase(host);

            //string baseUrl = "https://localhost:44374";
            //// string baseUrl = "https://localhost:5001";
            //string tmp = hostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);

            //var file = System.IO.File.Create("./ClientApp/src/baseUrl.js");
            //using (var writer = new System.IO.StreamWriter(file))
            //{
            //    writer.WriteLine("window.baseUrl = '" + baseUrl + "';");
            //}

            //file = System.IO.File.Create("./ClientApp/dist/src/baseUrl.js");
            //using (var writer = new System.IO.StreamWriter(file))
            //{
            //    writer.WriteLine("window.baseUrl = '" + baseUrl + "';");
            //}

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void InitializeDatabase(IWebHost host)
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                if (!services.GetService<ApplicationDbContext>().AllMigrationsApplied())
                {
                    services.GetService<ApplicationDbContext>().Database.Migrate();
                }

                IUserManagementService userManager = services.GetRequiredService<IUserManagementService>();
                var usersCount = userManager.GetAllUsersCountAsync("").Result;
                if (usersCount == 0)
                {
                    RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    foreach (RolePair role in RoleHelpers.Roles)
                    {
                        if (!roleManager.RoleExistsAsync(role.Name).Result)
                        {
                            var idRole = new IdentityRole(role.Name);
                            roleManager.CreateAsync(idRole).Wait();
                        }
                    }

                    // Create admin user
                    ApplicationUser adminUser = new ApplicationUser();
                    adminUser.UserName = "admin@domain.com";
                    adminUser.Email = "admin@domain.com";
                    adminUser.FirstName = "AdminFirst";
                    adminUser.LastName = "AdminLast";
                    adminUser.EmailConfirmed = true;
                    adminUser.Approved = true;

                    userManager.AddUserAsync(adminUser, "admin", "admin").Wait(); // username = "admin", password = "admin"

                    ApplicationUser employeeUser = new ApplicationUser();
                    employeeUser.UserName = "employee@domain.com";
                    employeeUser.Email = "employee@domain.com";
                    employeeUser.FirstName = "EmployeeFirst";
                    employeeUser.LastName = "EmployeeLast";
                    employeeUser.EmailConfirmed = true;
                    employeeUser.Approved = true;

                    userManager.AddUserAsync(employeeUser, "employee", "employee").Wait();

                    var userItem = new ApplicationUser();

                    // Create 5 users
                    for (int i = 1; i < 25; i++)
                    {
                        userItem = new ApplicationUser();
                        userItem.UserName = "user" + i + "@domain.com";
                        userItem.Email = "user" + i + "@domain.com";
                        userItem.FirstName = "USER" + i + "FIRST";
                        userItem.LastName = "USER" + i + "LAST";
                        userItem.EmailConfirmed = true;
                        userItem.Approved = true;
                        userManager.AddUserAsync(userItem, "user", "Applicant").Wait();
                    }
                }
            }
        }
    }
}
