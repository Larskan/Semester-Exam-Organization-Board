using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.Service;
using Moq;
using OrganizationBoard.DTO;
using OrganizationBoard.IService;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class AdminServiceTest
    {
        private Mock<IBCryptService> _mockBCryptService;
        public AdminServiceTest()
        {
            _mockBCryptService = new Mock<IBCryptService>();
        }



        private static OBDbContext GetInMemoryDbContext(string dbName = "TeamServiceTests")
        {
            var options = new DbContextOptionsBuilder<OBDbContext>()
                .UseInMemoryDatabase(databaseName: dbName).Options;

            var context = new OBDbContext(options);
            // The Seed Data from EFrameWork.Data/OBDbContext is not automatically reached, so we have to add some data manually.
            context.OrganizationTables.AddRange(
                new Organization { OrganizationID = 1, OrganizationName = "OrgOne" },
                new Organization { OrganizationID = 2, OrganizationName = "OrgTwo" }
            );
            context.UserTables.AddRange(
                new User { UserID = 1, RoleID = 1, Email = "Test1@email.com", Password = "Lars123!", OrganizationID = 1 }, //Admin
                new User { UserID = 2, RoleID = 2, Email = "Test2@email.com", Password = "Lars123!", OrganizationID = 1, TeamID = 1 }, // Leader
                new User { UserID = 3, RoleID = 3, Email = "Test3@email.com", Password = "Lars123!", OrganizationID = 1, TeamID = 1 }, // Member
                new User { UserID = 4, RoleID = 2, Email = "Test4@email.com", Password = "Lars123!", OrganizationID = 1, TeamID = 2 },  // Leader
                new User { UserID = 5, RoleID = 3, Email = "Test5@email.com", Password = "Lars123!", OrganizationID = 1 } //Member without team
            );
            context.SaveChanges();
            return context;
        }

        #region Tests for CreateUser
        // Test: Admin as invalid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task CreateUser_Returns403_IfUserNotAdmin()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotAdminTest");
            var mockBCrypt = new Mock<IBCryptService>();
            var service = new AdminService(context, mockBCrypt.Object);

            // Act
            var result = await service.CreateUser(new UserCreateDto { }, requestingAdminId: 2);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }
        // Test: Email already exist = 400
        [Fact]
        public async System.Threading.Tasks.Task CreateUser_Returns400_IfEmailAlreadyExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("EmailExistsTest");
            var mockBCrypt = new Mock<IBCryptService>();

            var service = new AdminService(context, mockBCrypt.Object);

            // Act
            var result = await service.CreateUser(new UserCreateDto
            {
                Email = "Test3@email.com",
            }, requestingAdminId: 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Email already exists", result.Message);
        }
        // Test: Admin as valid user and email exists, creating new user
        [Fact]
        public async System.Threading.Tasks.Task CreateUser_Success_ValidAdminAndValidEmail()
        {
            // Arrange
            var context = GetInMemoryDbContext("EmailExistsTest");
            var mockBCrypt = new Mock<IBCryptService>();
            mockBCrypt.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("");

            var service = new AdminService(context, mockBCrypt.Object);

            // Act
            var result = await service.CreateUser(new UserCreateDto
            {
                Email = "Test6@email.com",
                Password = "Lars123!",
                RoleID = 3,
                OrganizationID = 1
            }, requestingAdminId: 1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("User created successfully", result.Message);
        }
        // Test: Exception in try/catch = 500
        [Fact]
        public async System.Threading.Tasks.Task CreateUser_Returns500_IfExceptionOccurs()
        {
            // Arrange
            var context = GetInMemoryDbContext("ExceptionTest");
            var mockBCrypt = new Mock<IBCryptService>();

            // Force HashPassword to throw.
            mockBCrypt.Setup(s => s.HashPassword(It.IsAny<string>())).Throws(new Exception("Hashing Failure"));

            var service = new AdminService(context, mockBCrypt.Object);

            // Act
            var result = await service.CreateUser(new UserCreateDto { Password = "Lars123!" }, requestingAdminId: 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
        #endregion Tests for CreateUser

        #region Tests for UpdateUser
        // Test: existingUser as null = 404
        [Fact]
        public async System.Threading.Tasks.Task UpdateUser_Returns404_IfUserNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("UserNotFoundTest");
            var service = new AdminService(context, _mockBCryptService.Object);

            // Act
            var result = await service.UpdateUser(new UserCreateDto
            {
                UserID = 999,
            }, requestingAdminId: 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Test: New email matches existing email = 400
        [Fact]
        public async System.Threading.Tasks.Task UpdateUser_Returns400_IfEmailAlreadyExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("EmailConflictTest");
            var service = new AdminService(context, _mockBCryptService.Object);

            // Act
            var result = await service.UpdateUser(new UserCreateDto
            {
                UserID = 3,
                Email = "Test3@email.com"
            }, requestingAdminId: 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        // Test: Successsfully updating user
        [Fact]
        public async System.Threading.Tasks.Task UpdateUser_SuccessfullyUpdatesUser()
        {
            // Arrange
            var context = GetInMemoryDbContext("SuccessfulUpdateTest");
            _mockBCryptService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("");
            var service = new AdminService(context, _mockBCryptService.Object);

            // Act
            var result = await service.UpdateUser(new UserCreateDto
            {
                UserID = 3,
                Email = "leader@example.com",
                RoleID = 2, //promotion to leader
            }, requestingAdminId: 1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("User updated successfully", result.Message);
        }
        #endregion Tests for UpdateUser

        #region Tests for DeleteUser
        // Test: existingUser as valid user, deleting user. 
        [Fact]
        public async System.Threading.Tasks.Task DeleteUser_SuccessfullyDeleteUser()
        {
            // Arrange
            var context = GetInMemoryDbContext("SuccessfulDeleteTest");
            var service = new AdminService(context, _mockBCryptService.Object);

            // Act
            var result = await service.DeleteUser(3, 1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("User deleted successfully", result.Message);
        }
        #endregion Tests for DeleteUser

        #region Tests for GetUser
        // Test: Admin as valid user, getting user
        [Fact]
        public async System.Threading.Tasks.Task GetUser_SuccessfullyGetUser()
        {
            // Arrange
            var context = GetInMemoryDbContext("SuccessfulGetUserTest");
            var service = new AdminService(context, _mockBCryptService.Object);

            // Act
            var result = await service.GetUser(3, 1);

            // Assert
            Assert.True(result.IsSuccess);
        }
        #endregion Tests for GetUser

        #region Tests for GetAllUsers
        // Test: Admin as valid user, getting all users
        [Fact]
        public async System.Threading.Tasks.Task GetAllUsers_SuccessfullyGetAllUsers()
        {
            // Arrange
            var context = GetInMemoryDbContext("SuccessfulGetAllUsersTest");
            var service = new AdminService(context, _mockBCryptService.Object);

            // Act
            var result = await service.GetAllUsers(1);

            // Assert
            Assert.True(result.IsSuccess);
        }
        #endregion Tests for GetAllUsers

        #region Tests for UpdateOrganization
        // Test: existingOrg as null = 404
        [Fact]
        public async System.Threading.Tasks.Task UpdateOrganization_Returns404_OrganizationNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("OrgNotFoundTest");
            var service = new AdminService(context, _mockBCryptService.Object);

            // Act
            var result = await service.UpdateOrganization(new Organization
            {
                OrganizationID = 99,
                OrganizationName = "OrgOne"
            }, requestingAdminId: 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);

        }
        // Test: existingOrg as valid org
        [Fact]
        public async System.Threading.Tasks.Task UpdateOrganization_SuccessfullyUpdateOrg()
        {
            // Arrange
            var context = GetInMemoryDbContext("SuccessfulUpdateOrgTest");
            var service = new AdminService(context, _mockBCryptService.Object);

            // Act
            var result = await service.UpdateOrganization(new Organization
            {
                OrganizationID = 1,
                OrganizationName = "NewCoolerOrg"
            }, requestingAdminId: 1);
            var updatedOrg = await context.OrganizationTables.FindAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Organization updated successfully", result.Message);
            Assert.Equal("NewCoolerOrg", updatedOrg.OrganizationName);
        }
        #endregion Tests for UpdateOrganization
    }
}