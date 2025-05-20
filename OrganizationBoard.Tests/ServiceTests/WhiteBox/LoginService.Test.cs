using System;
using System.Threading.Tasks;
using EFramework.Data;
using EFrameWork.Model;
using Microsoft.EntityFrameworkCore;
using Model;
using OrganizationBoard.DTO;
using OrganizationBoard.IService;
using OrganizationBoard.Service;
using Xunit;
using System.Linq;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class LoginServiceTest
    {
        private readonly IBCryptService _bCryptService;
        private readonly IRsaService _rsaService;
        private readonly LoginService _loginService;
        private readonly OBDbContext _dbContext;

        public LoginServiceTest()
        {
            _dbContext = TestDbContextFactory.Create();
            _bCryptService = new BCryptService();
            _rsaService = new RsaService();
            _loginService = new LoginService(_dbContext, _bCryptService, _rsaService);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test user
            var hashedPassword = _bCryptService.HashPassword("testPassword");
            var user = new User 
            { 
                Email = "test@test.com", 
                Password = hashedPassword, 
                Role = new Role { RoleName = "Admin" },
                OrganizationID = 1
            };

            _dbContext.UserTables.Add(user);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async System.Threading.Tasks.Task UserCheck_WhenEmailExistsAndPasswordValid_ShouldReturnUser()
        {
            // Arrange
            var loginDto = new LoginDto 
            { 
                Email = "test@test.com", 
                Password = _rsaService.EncryptInternal("testPassword") // Use EncryptInternal
            };

            // Act
            var result = await _loginService.UserCheck(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
            Assert.NotNull(result.Role);
            Assert.Equal("Admin", result.Role.RoleName);
        }

        [Fact]
        public async System.Threading.Tasks.Task UserCheck_WhenEmailDoesNotExist_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto 
            { 
                Email = "nonexistent@test.com", 
                Password = _rsaService.EncryptInternal("testPassword") // Use EncryptInternal
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _loginService.UserCheck(loginDto));
        }

        [Fact]
        public async System.Threading.Tasks.Task UserCheck_WhenPasswordInvalid_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto 
            { 
                Email = "test@test.com", 
                Password = _rsaService.EncryptInternal("wrongPassword") // Use EncryptInternal
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _loginService.UserCheck(loginDto));
        }

        [Fact]
        public async System.Threading.Tasks.Task UserCheck_WhenRsaDecryptionFails_ShouldThrowApplicationException()
        {
            // Arrange
            var loginDto = new LoginDto 
            { 
                Email = "test@test.com", 
                Password = "invalid-encrypted-data" // This will cause RSA decryption to fail
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _loginService.UserCheck(loginDto));
        }

        [Fact]
        public async System.Threading.Tasks.Task UserCheck_WhenRsaDecryptionFailsWithValidBase64_ShouldThrowApplicationException()
        {
            // Arrange
            var loginDto = new LoginDto 
            { 
                Email = "test@test.com", 
                Password = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }) // Valid base64 but invalid RSA data
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _loginService.UserCheck(loginDto));
        }

        [Fact]
        public async System.Threading.Tasks.Task UserCheck_WhenEmailIsNull_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto 
            { 
                Email = null,
                Password = _rsaService.EncryptInternal("testPassword")
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _loginService.UserCheck(loginDto));
        }

        [Fact]
        public async System.Threading.Tasks.Task UserCheck_WhenPasswordIsNull_ShouldThrowApplicationException()
        {
            // Arrange
            var loginDto = new LoginDto 
            { 
                Email = "test@test.com",
                Password = null
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _loginService.UserCheck(loginDto));
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateAccountAndOrg_WhenEmailDoesNotExistAndAdminRoleExists_ShouldCreateAccount()
        {
            // Arrange
            var dto = new AccountAndOrgDto 
            { 
                Email = "new@test.com", 
                Password = "password", 
                OrgName = "TestOrg" 
            };

            // Act
            await _loginService.CreateAccountAndOrg(dto);

            // Assert
            var createdUser = await _dbContext.UserTables
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email == "new@test.com");
            
            Assert.NotNull(createdUser);
            Assert.Equal("new@test.com", createdUser.Email);
            Assert.NotNull(createdUser.Organization);
            Assert.Equal("TestOrg", createdUser.Organization.OrganizationName);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateAccountAndOrg_WhenEmailExists_ShouldThrowApplicationException()
        {
            // Arrange
            var dto = new AccountAndOrgDto 
            { 
                Email = "test@test.com", // Using existing email
                Password = "password", 
                OrgName = "TestOrg" 
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _loginService.CreateAccountAndOrg(dto));
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateAccountAndOrg_WhenAdminRoleDoesNotExist_ShouldThrowApplicationException()
        {
            // Arrange
            var dto = new AccountAndOrgDto 
            { 
                Email = "new@test.com", 
                Password = "password", 
                OrgName = "TestOrg" 
            };

            // Create a new context without the admin role
            var options = new DbContextOptionsBuilder<OBDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var contextWithoutAdminRole = new OBDbContext(options);
            var loginServiceWithoutAdminRole = new LoginService(contextWithoutAdminRole, _bCryptService, _rsaService);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => loginServiceWithoutAdminRole.CreateAccountAndOrg(dto));
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateAccountAndOrg_WhenEmailIsNull_ShouldThrowApplicationException()
        {
            // Arrange
            var dto = new AccountAndOrgDto 
            { 
                Email = null,
                Password = "password", 
                OrgName = "TestOrg" 
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _loginService.CreateAccountAndOrg(dto));
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateAccountAndOrg_WhenPasswordIsNull_ShouldThrowApplicationException()
        {
            // Arrange
            var dto = new AccountAndOrgDto 
            { 
                Email = "new@test.com",
                Password = null, 
                OrgName = "TestOrg" 
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _loginService.CreateAccountAndOrg(dto));
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateAccountAndOrg_WhenOrgNameIsNull_ShouldThrowApplicationException()
        {
            // Arrange
            var dto = new AccountAndOrgDto 
            { 
                Email = "new@test.com",
                Password = "password", 
                OrgName = null 
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _loginService.CreateAccountAndOrg(dto));
        }
    }
}