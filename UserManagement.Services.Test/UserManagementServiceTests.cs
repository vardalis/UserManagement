using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using UserManagement.Services;
using Microsoft.AspNetCore.Identity;
using UserManagement.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UserManagement.Services.Helpers;
using static UserManagement.Services.Helpers.RoleHelpers;
using Moq;
using System.Linq;

namespace UserManagement.Services
{
    public class UserManagementServiceTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "GenericDatabase")
                .Options;

        private ApplicationDbContext testingContext;
        private UserManager<ApplicationUser> testingUserManager;
        private RoleManager<IdentityRole> testingRoleManager;
        private UserManagementService userManagementService;

        public UserManagementServiceTests()
        {
            testingContext = new ApplicationDbContext(options);
            testingUserManager = CreateUserManager(testingContext);
            testingRoleManager = CreateRoleManager(testingContext);
            userManagementService = new UserManagementService(testingContext, CreateUserManager(testingContext), CreateRoleManager(testingContext));
        }

        public void Dispose()
        {
            testingContext.Database.EnsureDeleted();
            testingContext.Dispose();
            testingUserManager.Dispose();
            testingRoleManager.Dispose();
        }

        private UserManager<ApplicationUser> CreateUserManager(ApplicationDbContext context)
        {
            var userStore = new UserStore<ApplicationUser>(context);
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var userValidators = new List<UserValidator<ApplicationUser>> { new UserValidator<ApplicationUser>() };
            var passwordValidators = new List<PasswordValidator<ApplicationUser>> { new PasswordValidator<ApplicationUser>() };
            var userLogger = (new LoggerFactory()).CreateLogger<UserManager<ApplicationUser>>();

            //var identityOptions = new IdentityOptions();

            //// Password settings
            //identityOptions.Password.RequireDigit = false;
            //identityOptions.Password.RequiredLength = 1;
            //identityOptions.Password.RequireNonAlphanumeric = false;
            //identityOptions.Password.RequireUppercase = false;
            //identityOptions.Password.RequireLowercase = false;
            //identityOptions.Password.RequiredUniqueChars = 1;

            //// Lockout settings
            //identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            //identityOptions.Lockout.MaxFailedAccessAttempts = 10;
            //identityOptions.Lockout.AllowedForNewUsers = true;

            //// User settings
            //identityOptions.User.RequireUniqueEmail = true;

            // var identityOptionsWrapper = new Microsoft.Extensions.Options.OptionsWrapper<IdentityOptions>(identityOptions);

            return new UserManager<ApplicationUser>(userStore, null, passwordHasher, userValidators, passwordValidators,
                null, null, null, userLogger);
        }

        private RoleManager<IdentityRole> CreateRoleManager(ApplicationDbContext context)
        {
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleValidators = new List<RoleValidator<IdentityRole>> { new RoleValidator<IdentityRole>() };
            var roleLogger = (new LoggerFactory()).CreateLogger<RoleManager<IdentityRole>>();

            return new RoleManager<IdentityRole>(roleStore, roleValidators, null, null, roleLogger);
        }

        [Fact]
        public async Task GetAllUsersCountAsync_TwoUsers_ReturnsOneMatchingUserCount()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            using (var roleManager = CreateRoleManager(arrangeContext))
            {
                var user1 = new ApplicationUser
                {
                    FirstName = "Dimitris",
                    LastName = "Vardalis",
                    Email = "vardalis@gmail.com",
                    UserName = "vardalis@gmail.com"
                };

                await userManager.CreateAsync(user1, "accEPTable123!@#Pass");

                var user2 = new ApplicationUser
                {
                    FirstName = "TestFirst",
                    LastName = "TestLast",
                    Email = "test@domain.com",
                    UserName = "test@domain.com"
                };

                await userManager.CreateAsync(user2, "accEPTable123!@#Pass");
            }

            // Act
            var allUsersCount = userManagementService.GetAllUsersCountAsync("Test").Result;

            // Assert
            Assert.Equal(1, allUsersCount);
        }

        // Mock the User and Role Managers instead of creating real ones
        // Doesn't work for faking userManagement.Users
        //[Fact]
        //public void GetAllUsersCountAsync_TwoUsers_ReturnsOneMatchingUserCount_Version2()
        //{
        //    // Arrange
        //    var users = new List<ApplicationUser>
        //    {
        //        new ApplicationUser
        //        {
        //            FirstName = "Dimitris",
        //            LastName = "Vardalis",
        //            Email = "vardalis@gmail.com",
        //            UserName = "vardalis@gmail.com"
        //        },
        //        new ApplicationUser
        //        {
        //            FirstName = "TestFirst",
        //            LastName = "TestLast",
        //            Email = "test@domain.com",
        //            UserName = "test@domain.com"
        //        }
        //    };

        //    // Act
        //    var allUsersCount = userManagementService.GetAllUsersCountAsync("Test").Result;

        //    // Assert
        //    Assert.Equal(1, allUsersCount);
        //}

        [Fact]
        public async Task AddUserAsync_EmptyDatabase_UserAdded()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var roleManager = CreateRoleManager(arrangeContext))
            {
                var employeeRole = new IdentityRole("employee");
                await roleManager.CreateAsync(employeeRole);
            }

            var addUser = new ApplicationUser
            {
                FirstName = "Dimitris",
                LastName = "Vardalis",
                Email = "vardalis@gmail.com",
                UserName = "vardalis@gmail.com"
            };

            // Act
            var identityResult = await userManagementService.AddUserAsync(addUser, "accEPTable123!@#Pass", "employee");

            // Assert
            Assert.Collection(await testingContext.Users.ToListAsync(), user => Assert.Equal("vardalis@gmail.com", user.Email));
        }

        [Fact]
        public async Task GetUserRoleAsync_OneUserInDatabase_ReturnsCorrectUserRole()
        {
            // Arrange
            var user1 = new ApplicationUser
            {
                FirstName = "Dimitris",
                LastName = "Vardalis",
                Email = "vardalis@gmail.com",
                UserName = "vardalis@gmail.com"
            };

            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            using (var roleManager = CreateRoleManager(arrangeContext))
            {
                foreach (RolePair role in RoleHelpers.Roles)
                {
                    if (!roleManager.RoleExistsAsync(role.Name).Result)
                    {
                        var idRole = new IdentityRole(role.Name);
                        roleManager.CreateAsync(idRole).Wait();
                    }
                }

                await userManager.CreateAsync(user1, "accEPTable123!@#Pass");
                await userManager.AddToRoleAsync(user1, "employee");
            }

            // Act
            string result = await userManagementService.GetUserRoleAsync(user1.Id, false);

            // Assert
            Assert.Equal("Employee", result);
        }

        [Fact]
        public async Task GetUsersAsync_ThreeUsersInDatabase_ReturnsLnameOrder()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            {
                var a = new ApplicationUser
                {
                    FirstName = "a",
                    LastName = "b",
                    Email = "c@b.c",
                    UserName = "c@b.c"
                };

                await userManager.CreateAsync(a, "accEPTable123!@#Pass");

                var b = new ApplicationUser
                {
                    FirstName = "b",
                    LastName = "c",
                    Email = "a@b.c",
                    UserName = "a@b.c"
                };

                await userManager.CreateAsync(b, "accEPTable123!@#Pass");

                var c = new ApplicationUser
                {
                    FirstName = "c",
                    LastName = "a",
                    Email = "b@b.c",
                    UserName = "b@b.c"
                };

                await userManager.CreateAsync(c, "accEPTable123!@#Pass");
            }

            // Act
            var result = await userManagementService.GetUsersAsync(0, 3, "Lname", null);

            // Assert
            Assert.Collection(result, 
                user => Assert.Equal("a", user.LastName),
                user => Assert.Equal("b", user.LastName),
                user => Assert.Equal("c", user.LastName) );
        }

        [Fact]
        public async Task GetUsersAsync_ThreeUsersInDatabase_ReturnsLnameDescOrder()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            {
                var a = new ApplicationUser
                {
                    FirstName = "a",
                    LastName = "b",
                    Email = "c@b.c",
                    UserName = "c@b.c"
                };

                await userManager.CreateAsync(a, "accEPTable123!@#Pass");

                var b = new ApplicationUser
                {
                    FirstName = "b",
                    LastName = "c",
                    Email = "a@b.c",
                    UserName = "a@b.c"
                };

                await userManager.CreateAsync(b, "accEPTable123!@#Pass");

                var c = new ApplicationUser
                {
                    FirstName = "c",
                    LastName = "a",
                    Email = "b@b.c",
                    UserName = "b@b.c"
                };

                await userManager.CreateAsync(c, "accEPTable123!@#Pass");
            }

            // Act
            var result = await userManagementService.GetUsersAsync(0, 3, "Lname_desc", null);

            // Assert
            Assert.Collection(result,
                user => Assert.Equal("c", user.LastName),
                user => Assert.Equal("b", user.LastName),
                user => Assert.Equal("a", user.LastName));
        }

        [Fact]
        public async Task GetUsersAsync_ThreeUsersInDatabase_ReturnsFnameOrder()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            {
                var a = new ApplicationUser
                {
                    FirstName = "a",
                    LastName = "b",
                    Email = "c@b.c",
                    UserName = "c@b.c"
                };

                await userManager.CreateAsync(a, "accEPTable123!@#Pass");

                var b = new ApplicationUser
                {
                    FirstName = "b",
                    LastName = "c",
                    Email = "a@b.c",
                    UserName = "a@b.c"
                };

                await userManager.CreateAsync(b, "accEPTable123!@#Pass");

                var c = new ApplicationUser
                {
                    FirstName = "c",
                    LastName = "a",
                    Email = "b@b.c",
                    UserName = "b@b.c"
                };

                await userManager.CreateAsync(c, "accEPTable123!@#Pass");
            }

            // Act
            var result = await userManagementService.GetUsersAsync(0, 3, "Fname", null);

            // Assert
            Assert.Collection(result,
                user => Assert.Equal("a", user.FirstName),
                user => Assert.Equal("b", user.FirstName),
                user => Assert.Equal("c", user.FirstName));
        }

        [Fact]
        public async Task GetUsersAsync_ThreeUsersInDatabase_ReturnsFnameDescOrder()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            {
                var a = new ApplicationUser
                {
                    FirstName = "a",
                    LastName = "b",
                    Email = "c@b.c",
                    UserName = "c@b.c"
                };

                await userManager.CreateAsync(a, "accEPTable123!@#Pass");

                var b = new ApplicationUser
                {
                    FirstName = "b",
                    LastName = "c",
                    Email = "a@b.c",
                    UserName = "a@b.c"
                };

                await userManager.CreateAsync(b, "accEPTable123!@#Pass");

                var c = new ApplicationUser
                {
                    FirstName = "c",
                    LastName = "a",
                    Email = "b@b.c",
                    UserName = "b@b.c"
                };

                await userManager.CreateAsync(c, "accEPTable123!@#Pass");
            }

            // Act
            var result = await userManagementService.GetUsersAsync(0, 3, "Fname_desc", null);

            // Assert
            Assert.Collection(result,
                user => Assert.Equal("c", user.FirstName),
                user => Assert.Equal("b", user.FirstName),
                user => Assert.Equal("a", user.FirstName));
        }

        [Fact]
        public async Task GetUsersAsync_ThreeUsersInDatabase_ReturnsEmailOrder()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            {
                var a = new ApplicationUser
                {
                    FirstName = "a",
                    LastName = "a",
                    Email = "a@b.c",
                    UserName = "a@b.c"
                };

                await userManager.CreateAsync(a, "accEPTable123!@#Pass");

                var b = new ApplicationUser
                {
                    FirstName = "b",
                    LastName = "b",
                    Email = "b@b.c",
                    UserName = "b@b.c"
                };

                await userManager.CreateAsync(b, "accEPTable123!@#Pass");

                var c = new ApplicationUser
                {
                    FirstName = "c",
                    LastName = "c",
                    Email = "c@b.c",
                    UserName = "c@b.c"
                };

                await userManager.CreateAsync(c, "accEPTable123!@#Pass");
            }

            // Act
            var result = await userManagementService.GetUsersAsync(0, 3, "Email", null);

            // Assert
            Assert.Collection(result,
                user => Assert.Equal("a@b.c", user.Email),
                user => Assert.Equal("b@b.c", user.Email),
                user => Assert.Equal("c@b.c", user.Email));
        }

        [Fact]
        public async Task GetUsersAsync_ThreeUsersInDatabase_ReturnsEmailDescOrder()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            {
                var a = new ApplicationUser
                {
                    FirstName = "a",
                    LastName = "b",
                    Email = "c@b.c",
                    UserName = "c@b.c"
                };

                await userManager.CreateAsync(a, "accEPTable123!@#Pass");

                var b = new ApplicationUser
                {
                    FirstName = "b",
                    LastName = "c",
                    Email = "a@b.c",
                    UserName = "a@b.c"
                };

                await userManager.CreateAsync(b, "accEPTable123!@#Pass");

                var c = new ApplicationUser
                {
                    FirstName = "c",
                    LastName = "a",
                    Email = "b@b.c",
                    UserName = "b@b.c"
                };

                await userManager.CreateAsync(c, "accEPTable123!@#Pass");
            }

            // Act
            var result = await userManagementService.GetUsersAsync(0, 3, "Email_desc", null);

            // Assert
            Assert.Collection(result,
                user => Assert.Equal("c@b.c", user.Email),
                user => Assert.Equal("b@b.c", user.Email),
                user => Assert.Equal("a@b.c", user.Email));
        }

        [Fact]
        public async Task GetUsersAsync_ThreeUsersInDatabase_ReturnsApprovedOrder()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            {
                var a = new ApplicationUser
                {
                    FirstName = "a",
                    LastName = "b",
                    Email = "c@b.c",
                    UserName = "c@b.c",
                    Approved = true
                };

                await userManager.CreateAsync(a, "accEPTable123!@#Pass");

                var b = new ApplicationUser
                {
                    FirstName = "b",
                    LastName = "c",
                    Email = "a@b.c",
                    UserName = "a@b.c",
                    Approved = false
                };

                await userManager.CreateAsync(b, "accEPTable123!@#Pass");
            }

            // Act
            var result = await userManagementService.GetUsersAsync(0, 3, "Approved", null);

            // Assert
            Assert.Collection(result,
                user => Assert.False(user.Approved),
                user => Assert.True(user.Approved) );
        }

        [Fact]
        public async Task GetUsersAsync_ThreeUsersInDatabase_ReturnsApprovedDescOrder()
        {
            // Arrange
            using (var arrangeContext = new ApplicationDbContext(options))
            using (var userManager = CreateUserManager(arrangeContext))
            {
                var a = new ApplicationUser
                {
                    FirstName = "a",
                    LastName = "b",
                    Email = "c@b.c",
                    UserName = "c@b.c",
                    Approved = true
                };

                await userManager.CreateAsync(a, "accEPTable123!@#Pass");

                var b = new ApplicationUser
                {
                    FirstName = "b",
                    LastName = "c",
                    Email = "a@b.c",
                    UserName = "a@b.c",
                    Approved = false
                };

                await userManager.CreateAsync(b, "accEPTable123!@#Pass");
            }

            // Act
            var result = await userManagementService.GetUsersAsync(0, 3, "Approved_desc", null);

            // Assert
            Assert.Collection(result,
                user => Assert.True(user.Approved),
                user => Assert.False(user.Approved));
        }
    }
}
