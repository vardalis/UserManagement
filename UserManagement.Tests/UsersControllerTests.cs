using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Entities;
using UserManagement.Helpers;
using UserManagement.Models;
using UserManagement.Services;
using Xunit;

namespace UserManagement.Controllers
{
    public class UsersControllerTests
    {
        public UsersControllerTests()
        {

        }

        [Fact]
        public async Task GetUsers_TwoUsers_ReturnsListOfUsers()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            var a = new ApplicationUser
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
                UserName = "c@b.c"
            };

            var b = new ApplicationUser
            {
                Id = "b",
                FirstName = "b",
                LastName = "c",
                Email = "a@b.c",
                UserName = "a@b.c"
            };

            mockUmService.Setup(umService => umService.GetAllUsersCountAsync("a"))
                .ReturnsAsync(2)
                .Verifiable();

            mockUmService.Setup(umService => umService.GetUsersAsync(0, 2, "Fname", "a"))
                .ReturnsAsync(new List<ApplicationUser>() { a, b })
                .Verifiable();

            mockUmService.Setup(umService => umService.GetUserRoleAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("Employee")
                .Verifiable();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            // Act
            var result = await controller.GetUsers(0, 2, "Fname", "a");

            // Assert
            var actionResult = Assert.IsType<ActionResult<PageUsersModel>>(result);
            var returnValue = Assert.IsType<PageUsersModel>(actionResult.Value);
            mockUmService.Verify();

            Assert.Equal(2, returnValue.TotalUsers);
            Assert.Collection(returnValue.Users,
                user => Assert.Equal("b", user.LastName),
                user => Assert.Equal("c", user.LastName)
            );
        }

        [Fact]
        public async Task GetUser_UserNotExists_ReturnsNotFound()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            mockUmService.Setup(umService => umService.FindUserAsync("a"))
                .ReturnsAsync((ApplicationUser)null)
                .Verifiable();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            // Act
            var result = await controller.GetUser("a");

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserModel>>(result);
            var returnValue = Assert.IsType<NotFoundResult>(actionResult.Result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task GetUser_UserExists_ReturnsUser()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            var a = new ApplicationUser
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
                UserName = "c@b.c"
            };

            mockUmService.Setup(umService => umService.FindUserAsync("a"))
                .ReturnsAsync(a)
                .Verifiable();

            mockUmService.Setup(umService => umService.GetUserRoleAsync("a", true))
                .ReturnsAsync("Employee")
                .Verifiable();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            // Act
            var result = await controller.GetUser("a");

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserModel>>(result);
            var returnValue = Assert.IsType<UserModel>(actionResult.Value);
            mockUmService.Verify();

            Assert.Equal("c@b.c", returnValue.Email);
        }

        [Fact]
        public async Task PostUser_ModelInvalid_ReturnsBadRequest()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            controller.ModelState.AddModelError("LastName", "Last name is missing.");

            var a = new UserModel
            {
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
            };

            // Act
            var result = await controller.PostUser(a);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserModel>>(result);
            var returnValue = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task PostUser_EmailInUse_ReturnsBadRequest()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            var a = new UserModel
            {
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
            };

            mockUmService.Setup(umService => umService.IsEmailInUseAsync("c@b.c"))
                .ReturnsAsync(true)
                .Verifiable();

            // Act
            var result = await controller.PostUser(a);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserModel>>(result);
            var returnValue = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task PostUser_NewUserCorrect_UserAddedAndReturned()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            var a = new UserModel
            {
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
                Password = "pass",
                Role = "role"
            };

            mockUmService.Setup(umService => umService.IsEmailInUseAsync("c@b.c"))
                .ReturnsAsync(false)
                .Verifiable();

            ApplicationUser user = mapper.Map<ApplicationUser>(a);

            mockUmService.Setup(umService => umService.AddUserAsync(It.IsAny<ApplicationUser>(), "pass", "role"))
                .ReturnsAsync(new IdentityResult())
                .Verifiable();

            // Act
            var result = await controller.PostUser(a);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserModel>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<ApplicationUser>(createdAtActionResult.Value);
            mockUmService.Verify();

            Assert.Equal("c@b.c", returnValue.Email);
        }

        [Fact]
        public async Task PutUser_ModelInvalid_ReturnsBadRequest()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            controller.ModelState.AddModelError("LastName", "Last name is missing.");

            var a = new UpdateUserModel
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
            };

            // Act
            var result = await controller.PutUser("a", a);

            // Assert
            var actionResult = Assert.IsType<BadRequestResult>(result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task PutUser_UserNotExist_ReturnsNotFound()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            var a = new UpdateUserModel
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
            };

            mockUmService.Setup(umService => umService.FindUserAsync("a"))
                .ReturnsAsync((ApplicationUser) null)
                .Verifiable();

            // Act
            var result = await controller.PutUser("a", a);

            // Assert
            var actionResult = Assert.IsType<NotFoundResult>(result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task PutUser_EmailInUse_ReturnsBadRequest()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfiles.UserProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mapper, mockHelperService.Object);

            var a = new UpdateUserModel
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
                Role = "employee"
            };

            ApplicationUser user = new ApplicationUser
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c"
            };

            mockUmService.Setup(umService => umService.FindUserAsync("a"))
                .ReturnsAsync(user)
                .Verifiable();

            mockUmService.Setup(umService => umService.IsEmailInUseAsync("c@b.c", "a"))
                .ReturnsAsync(true)
                .Verifiable();

            // Act
            var result = await controller.PutUser("a", a);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            mockUmService.Verify();
        }

        // This test introducing faking of the mapper instead of creating a real one
        [Fact]
        public async Task PutUser_UpdatedUserCorrect_ReturnsNoContent()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            //var mapperConfig = new MapperConfiguration(cfg =>
            //{
            //    cfg.AddProfile(new AutomapperProfiles.UserProfile());
            //});
            //var mapper = mapperConfig.CreateMapper();

            var mockMapper = new Mock<IMapper>();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mockMapper.Object, mockHelperService.Object);

            var aOld = new UpdateUserModel
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
                Role = "employee"
            };

            var aNew = new UpdateUserModel
            {
                Id = "a",
                FirstName = "b",
                LastName = "c",
                Email = "a@b.c",
                Role = "supervisor"
            };

            ApplicationUser oldUser = new ApplicationUser
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c"
            };

            ApplicationUser newUser = new ApplicationUser
            {
                Id = "a",
                FirstName = "b",
                LastName = "c",
                Email = "a@b.c"
            };

            mockUmService.Setup(umService => umService.FindUserAsync("a"))
                .ReturnsAsync(oldUser)
                .Verifiable();

            mockUmService.Setup(umService => umService.IsEmailInUseAsync("a@b.c", "a"))
                .ReturnsAsync(false)
                .Verifiable();

            mockMapper.Setup(mapper => mapper.Map(aNew, oldUser))
                .Callback<UpdateUserModel, ApplicationUser>((userModel, user) =>
                {
                    user.FirstName = userModel.FirstName;
                    user.LastName = userModel.LastName;
                    user.Email = userModel.Email;
                })
                .Returns(newUser)
                .Verifiable();

            mockUmService.Setup(umService => umService.UpdateUserAsync(oldUser, "supervisor", It.IsAny<byte[]>()))
                .ReturnsAsync(new IdentityResult())
                .Verifiable();

            // Act
            var result = await controller.PutUser("a", aNew);

            // Assert
            var actionResult = Assert.IsType<NoContentResult>(result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task PutUser_UserHasChangedInDatabase_ReturnsBadRequest()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockHelperService = new Mock<IHelperService>();

            var mockMapper = new Mock<IMapper>();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mockMapper.Object, mockHelperService.Object);

            var aOld = new UpdateUserModel
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c",
                Role = "employee"
            };

            var aNew = new UpdateUserModel
            {
                Id = "a",
                FirstName = "b",
                LastName = "c",
                Email = "a@b.c",
                Role = "supervisor"
            };

            ApplicationUser oldUser = new ApplicationUser
            {
                Id = "a",
                FirstName = "a",
                LastName = "b",
                Email = "c@b.c"
            };

            ApplicationUser newUser = new ApplicationUser
            {
                Id = "a",
                FirstName = "b",
                LastName = "c",
                Email = "a@b.c"
            };

            mockUmService.Setup(umService => umService.FindUserAsync("a"))
                .ReturnsAsync(oldUser)
                .Verifiable();

            mockUmService.Setup(umService => umService.IsEmailInUseAsync("a@b.c", "a"))
                .ReturnsAsync(false)
                .Verifiable();

            mockMapper.Setup(mapper => mapper.Map(aNew, oldUser))
                .Callback<UpdateUserModel, ApplicationUser>((userModel, user) =>
                {
                    user.FirstName = userModel.FirstName;
                    user.LastName = userModel.LastName;
                    user.Email = userModel.Email;
                })
                .Returns(newUser)
                .Verifiable();

            DbUpdateConcurrencyException dbUpdateConcurrencyException =
                new DbUpdateConcurrencyException("Exception",
                new List<IUpdateEntry>() { new Mock<IUpdateEntry>().Object } );

            mockUmService.Setup(umService => umService.UpdateUserAsync(oldUser, "supervisor", It.IsAny<byte[]>()))
                .ThrowsAsync(dbUpdateConcurrencyException)
                .Verifiable();

            mockHelperService.Setup(helperService => helperService.RetrieveEntity(dbUpdateConcurrencyException))
                .ReturnsAsync((PropertyValues)null);

            // Act
            var result = await controller.PutUser("a", aNew);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task DeleteUser_UserNotExists_ReturnsNotFound()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockMapper = new Mock<IMapper>();
            var mockHelperService = new Mock<IHelperService>();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mockMapper.Object, mockHelperService.Object);

            mockUmService.Setup(umService => umService.DeleteUserAsync("a"))
                .ReturnsAsync(IdentityResult.Failed())
                .Verifiable();

            // Act
            var result = await controller.DeleteUser("a");

            // Assert
            var actionResult = Assert.IsType<NotFoundResult>(result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task DeleteUser_UserExists_ReturnsNoContent()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockMapper = new Mock<IMapper>();
            var mockHelperService = new Mock<IHelperService>();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mockMapper.Object, mockHelperService.Object);

            mockUmService.Setup(umService => umService.DeleteUserAsync("a"))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            // Act
            var result = await controller.DeleteUser("a");

            // Assert
            var actionResult = Assert.IsType<NoContentResult>(result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task ChangeUserPassword_UserExists_ReturnsOk()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockMapper = new Mock<IMapper>();
            var mockHelperService = new Mock<IHelperService>();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mockMapper.Object, mockHelperService.Object);

            var user = new ApplicationUser();

            mockUmService.Setup(umService => umService.FindUserAsync("a"))
                .ReturnsAsync(user)
                .Verifiable();

            mockUmService.Setup(umService => umService.ChangePasswordAsync(user, "password"))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            var passwordChangeModel = new PasswordChangeModel()
            {
                Password = "password",
                ConfirmPassword = "passwsord"
            };

            // Act
            var result = await controller.ChangeUserPassword("a", passwordChangeModel);

            // Assert
            var actionResult = Assert.IsType<OkResult>(result);
            mockUmService.Verify();
        }

        [Fact]
        public async Task ChangePassword_UserNotExists_ReturnsNotFound()
        {
            // Arrange
            var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockUmService = new Mock<IUserManagementService>();
            var mockMapper = new Mock<IMapper>();
            var mockHelperService = new Mock<IHelperService>();

            var controller = new UsersController(mockConfiguration.Object, mockLogger.Object,
                mockUmService.Object, mockMapper.Object, mockHelperService.Object);

            mockUmService.Setup(umService => umService.FindUserAsync("a"))
                .ReturnsAsync((ApplicationUser) null)
                .Verifiable();

            var passwordChangeModel = new PasswordChangeModel()
            {
                Password = "password",
                ConfirmPassword = "passwsord"
            };

            // Act
            var result = await controller.ChangeUserPassword("a", passwordChangeModel);

            // Assert
            var actionResult = Assert.IsType<NotFoundResult>(result);
            mockUmService.Verify();
        }
    }
}
