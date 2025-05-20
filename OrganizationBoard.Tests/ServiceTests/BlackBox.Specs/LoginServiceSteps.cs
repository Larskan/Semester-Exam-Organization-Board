using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.DTO;
using OrganizationBoard.IService;
using OrganizationBoard.Service;
using Polly;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Collections.Generic;
using Reqnroll; // Added for List<T> used in DbSet mocking

namespace OrganizationBoard.Tests.Steps
{
    [Binding]
    public class LoginServiceSteps
    {
        private OBDbContext _db;
        private Mock<IBCryptService> _bCryptServiceMock;
        private Mock<IRsaService> _rsaServiceMock;
        private Mock<IAsyncPolicy> _retryPolicyMock;
        private LoginService _loginService;

        private User _expectedUser;
        private Exception _caughtException;
        private LoginDto _loginDto;
        private AccountAndOrgDto _accountAndOrgDto;

        public LoginServiceSteps()
        {
            // Initialize mocks for BCrypt and RSA services
            // These mocks are used to simulate the behavior of these services
            _bCryptServiceMock = new Mock<IBCryptService>();
            _rsaServiceMock = new Mock<IRsaService>();
            _retryPolicyMock = new Mock<IAsyncPolicy>();

            // Mock the retry policy to just execute the function without retrying
            _retryPolicyMock
                .Setup(p => p.ExecuteAsync(It.IsAny<Func<System.Threading.Tasks.Task>>()))
                .Returns((Func<System.Threading.Tasks.Task> func) => func());
            _retryPolicyMock
                .Setup(p => p.ExecuteAsync(It.IsAny<Func<Task<User>>>()))
                .Returns((Func<Task<User>> func) => func());

            // Initialize default context and service
            _db = TestDbContextFactory.Create();
            _loginService = new LoginService(
                _db,
                _bCryptServiceMock.Object,
                _rsaServiceMock.Object,
                _retryPolicyMock.Object
            );
        }

        [AfterScenario]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();

            // Re-initialize for the next scenario
            _db = TestDbContextFactory.Create();
            _loginService = new LoginService(
                _db,
                _bCryptServiceMock.Object,
                _rsaServiceMock.Object,
                _retryPolicyMock.Object
            );
        }

        // --- Existing steps (User, RSA, BCrypt) remain mostly the same ---

        [Given(@"a user with email ""(.*)"" and password ""(.*)"" exists in the database")]
        public async System.Threading.Tasks.Task GivenAUserWithEmailAndPasswordExistsInTheDatabase(string email, string password)
        {
            // Ensure the Admin role exists first, as User often depends on Role
            // This is a common pattern in BDD: set up prerequisites.
            await GivenTheRoleExistsInTheDatabase("Admin"); // Or "User" if that's the default role for these users

            var user = TestDataFactory.CreateUser(email: email, password: password);
            // Assign a role explicitly if it's not handled by the service itself on user creation
            // For example, if you just want to put a user into the DB for login check.
            user.RoleID = _db.RoleTables.Local.FirstOrDefault(r => r.RoleName == "Admin")?.RoleID ?? 1; // Assuming Admin is always ID 1
            user.Role = _db.RoleTables.Local.FirstOrDefault(r => r.RoleName == "Admin"); // Also set navigation property

            _db.UserTables!.Add(user);
            await _db.SaveChangesAsync();
            _expectedUser = user;
        }

        [Given(@"no user with email ""(.*)"" exists in the database")]
        public void GivenNoUserWithEmailExistsInTheDatabase(string email)
        {
            // No action needed, as the in-memory DB starts empty for each scenario
        }

        [Given(@"the RSA service will decrypt ""(.*)"" to ""(.*)""")]
        public void GivenTheRSAServiceWillDecryptTo(string encryptedPassword, string decryptedPassword)
        {
            _rsaServiceMock.Setup(rsa => rsa.Decrypt(encryptedPassword)).Returns(decryptedPassword);
        }

        [Given(@"the BCrypt service will verify ""(.*)"" against ""(.*)"" as (.*)")]
        public void GivenTheBCryptServiceWillVerifyAgainstAs(string decryptedPassword, string hashedPassword, bool result)
        {
            _bCryptServiceMock.Setup(b => b.VerifyPassword(decryptedPassword, hashedPassword)).Returns(result);
        }

        [When(@"the user attempts to log in with email ""(.*)"" and password ""(.*)""")]
        public async System.Threading.Tasks.Task WhenTheUserAttemptsToLogInWithEmailAndPassword(string email, string password)
        {
            _loginDto = TestDataFactory.CreateLoginDto(email, password);
            try
            {
                _caughtException = null;
                _expectedUser = await _loginService.UserCheck(_loginDto);
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [Then(@"the login should be successful and the user details are returned")]
        public void ThenTheLoginShouldBeSuccessfulAndTheUserDetailsAreReturned()
        {
            Assert.Null(_caughtException);
            Assert.NotNull(_expectedUser);
            Assert.Equal(_loginDto.Email, _expectedUser.Email);
        }

        [Given(@"the RSA service throws an exception when decrypting ""(.*)""")]
        public void GivenTheRSAServiceThrowsAnExceptionWhenDecrypting(string encryptedPassword)
        {
            _rsaServiceMock.Setup(rsa => rsa.Decrypt(encryptedPassword)).Throws(new Exception("Simulated RSA decryption error"));
        }

        [Given(@"the BCrypt service throws an exception when verifying ""(.*)"" against ""(.*)""")]
        public void GivenTheBCryptServiceThrowsAnExceptionWhenVerifyingAgainst(string decryptedPassword, string hashedPassword)
        {
            _bCryptServiceMock.Setup(b => b.VerifyPassword(decryptedPassword, hashedPassword)).Throws(new Exception("Simulated BCrypt verification error"));
        }

        [Then(@"an UnauthorizedAccessException should be thrown")]
        public void ThenAnUnauthorizedAccessExceptionShouldBeThrown()
        {
            Assert.NotNull(_caughtException);
            Assert.IsType<UnauthorizedAccessException>(_caughtException);
        }

        [Then(@"an ApplicationException with message ""(.*)"" should be thrown")]
        public void ThenAnApplicationExceptionWithMessageShouldBeThrown(string expectedMessage)
        {
            Assert.NotNull(_caughtException);
            var appEx = Assert.IsType<ApplicationException>(_caughtException);
            Assert.Equal(expectedMessage, appEx.Message);
        }

        [Given(@"a database error occurs when saving the organization ""(.*)""")]
        public async System.Threading.Tasks.Task GivenADatabaseErrorOccursWhenSavingTheOrganization(string orgName)
        {
            // First, ensure the Admin role exists in the *real* in-memory DB if this scenario
            // requires it for CreateAccountAndOrg to even start.
            await GivenTheRoleExistsInTheDatabase("Admin");

            var dbMockForError = new Mock<OBDbContext>();

            // Crucial: Set up other DbSet properties to return the *real* in-memory DbSets
            // so that non-failing operations (like checking for Admin role) work.
            dbMockForError.Setup(db => db.RoleTables).Returns(_db.RoleTables);
            dbMockForError.Setup(db => db.UserTables).Returns(_db.UserTables); // If user save comes after org, this will be needed later

            // Mock AddAsync and SaveChangesAsync for OrganizationTables to throw an exception
            dbMockForError.Setup(db => db.OrganizationTables!.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("Simulated DB error on organization save"));
            // Also mock SaveChangesAsync as it will be called after AddAsync
            dbMockForError.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("Simulated DB error on save changes for org"));

            _loginService = new LoginService(
                dbMockForError.Object,
                _bCryptServiceMock.Object,
                _rsaServiceMock.Object,
                _retryPolicyMock.Object
            );
        }

        [Given(@"a database error occurs when saving the user ""(.*)""")]
        public async System.Threading.Tasks.Task GivenADatabaseErrorOccursWhenSavingTheUser(string email)
        {
            // Ensure necessary prerequisites (Admin role, organization) are set up in the real DB
            await GivenTheRoleExistsInTheDatabase("Admin");
            await GivenTheOrganizationIsSuccessfullySaved("TempOrgForUserError"); // Create an organization that exists for the user to be linked to

            var dbMockForError = new Mock<OBDbContext>();

            // Crucial: Set up other DbSet properties to return the *real* in-memory DbSets
            // so that non-failing operations (like getting role or organization) work.
            dbMockForError.Setup(db => db.RoleTables).Returns(_db.RoleTables);
            dbMockForError.Setup(db => db.OrganizationTables).Returns(_db.OrganizationTables);

            // Mock AddAsync and SaveChangesAsync for UserTables to throw an exception
            dbMockForError.Setup(db => db.UserTables!.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("Simulated DB error on user save"));
            // Also mock SaveChangesAsync as it will be called after AddAsync
            dbMockForError.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("Simulated DB error on save changes for user"));

            _loginService = new LoginService(
                dbMockForError.Object,
                _bCryptServiceMock.Object,
                _rsaServiceMock.Object,
                _retryPolicyMock.Object
            );
        }

        // --- Common Setup Steps ---

        [Given(@"the ""(.*)"" role exists in the database")]
        public async System.Threading.Tasks.Task GivenTheRoleExistsInTheDatabase(string roleName)
        {
            // Check if the role already exists in the current in-memory DB context
            var existingRole = await _db.RoleTables.FirstOrDefaultAsync(r => r.RoleName == roleName);

            if (existingRole == null)
            {
                var role = TestDataFactory.CreateRole(roleName);
                _db.RoleTables!.Add(role);
                await _db.SaveChangesAsync();
                // Detach the entity after saving to prevent tracking issues in subsequent operations
                _db.Entry(role).State = EntityState.Detached;
            }
        }

        [Given(@"the ""(.*)"" role does not exist in the database")]
        public void GivenTheRoleDoesNotExitInTheDatabase(string roleName)
        {
            // No action needed, as the in-memory DB starts empty and we don't add this role.
            // If any fixed roles were added by default, you might need to ensure this role isn't one of them.
        }

        [When(@"a new account is created with email ""(.*)"", password ""(.*)"", and organization ""(.*)""")]
        public async System.Threading.Tasks.Task WhenANewAccountIsCreatedWithEmailPasswordAndOrganization(string email, string password, string orgName)
        {
            _accountAndOrgDto = TestDataFactory.CreateAccountAndOrgDto(email, password, orgName);
            try
            {
                _caughtException = null;
                await _loginService.CreateAccountAndOrg(_accountAndOrgDto);
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [Given(@"the organization ""(.*)"" is successfully saved")]
        public async System.Threading.Tasks.Task GivenTheOrganizationIsSuccessfullySaved(string orgName)
        {
            var existingOrg = await _db.OrganizationTables.FirstOrDefaultAsync(o => o.OrganizationName == orgName);
            if (existingOrg == null)
            {
                var org = TestDataFactory.CreateOrganization(orgName);
                _db.OrganizationTables!.Add(org);
                await _db.SaveChangesAsync();
                _db.Entry(org).State = EntityState.Detached; // Detach to ensure it's re-fetched by the service
            }
        }

        // --- Other steps remain unchanged ---

        [Given(@"the BCrypt service will hash ""(.*)"" to ""(.*)""")]
        public void GivenTheBCryptServiceWillHashTo(string rawPassword, string hashedPassword)
        {
            _bCryptServiceMock.Setup(b => b.HashPassword(rawPassword)).Returns(hashedPassword);
        }

        [Then(@"a new organization ""(.*)"" should be saved to the database")]
        public async System.Threading.Tasks.Task ThenANewOrganizationShouldBeSavedToTheDatabase(string orgName)
        {
            var organization = await _db.OrganizationTables!.FirstOrDefaultAsync(o => o.OrganizationName == orgName);
            Assert.NotNull(organization);
        }

        [Then(@"a new user ""(.*)"" with ""(.*)"" and ""(.*)"" role for ""(.*)"" should be saved to the database")]
        public async System.Threading.Tasks.Task ThenANewUserWithAndRoleForShouldBeSavedToTheDatabase(string email, string hashedPassword, string roleName, string orgName)
        {
            var user = await _db.UserTables!
                                .Include(u => u.Role)
                                .Include(u => u.Organization)
                                .FirstOrDefaultAsync(u => u.Email == email);

            Assert.NotNull(user);
            Assert.Equal(hashedPassword, user.Password);
            Assert.NotNull(user.Role);
            Assert.Equal(roleName, user.Role.RoleName);
            Assert.NotNull(user.Organization);
            Assert.Equal(orgName, user.Organization.OrganizationName);
        }
    }
}