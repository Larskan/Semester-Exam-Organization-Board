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
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Reqnroll;
using System.Threading;
using System.ComponentModel.DataAnnotations;


namespace OrganizationBoard.Tests.Steps
{
    [Binding]
    public class LoginServiceSteps
    {
        private OBDbContext _db;
        private Mock<IBCryptService> _bCryptServiceMock;
        private Mock<IRsaService> _rsaServiceMock;
        private LoginService _loginService;

        private User _expectedUser;
        private Exception _caughtException;
        private LoginDto _loginDto;
        private AccountAndOrgDto _accountAndOrgDto;

        public LoginServiceSteps()
        {
            _bCryptServiceMock = new Mock<IBCryptService>();
            _rsaServiceMock = new Mock<IRsaService>();

            _db = TestDbContextFactory.Create();
            _loginService = new LoginService(
                _db,
                _bCryptServiceMock.Object,
                _rsaServiceMock.Object
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
                _rsaServiceMock.Object
            );
        }

        [Given(@"a user with email ""(.*)"" and password ""(.*)"" exists in the database")]
        public async System.Threading.Tasks.Task GivenAUserWithEmailAndPasswordExistsInTheDatabase(string email, string password)
        {
            await GivenTheRoleExistsInTheDatabase("Admin");

            var user = TestDataFactory.CreateUser(email: email, password: password);
            user.RoleID = _db.RoleTables.Local.FirstOrDefault(r => r.RoleName == "Admin")?.RoleID ?? 1;
            user.Role = _db.RoleTables.Local.FirstOrDefault(r => r.RoleName == "Admin");

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
                Validator.ValidateObject(_loginDto, new ValidationContext(_loginDto), validateAllProperties: true);
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

        [Then(@"an UnauthorizedAccessException with message ""(.*)"" should be thrown")]
        public void ThenAnUnauthorizedAccessExceptionWithMessageShouldBeThrown(string expectedMessage)
        {
            Assert.NotNull(_caughtException);
            var appEx = Assert.IsType<UnauthorizedAccessException>(_caughtException);
            Assert.Equal(expectedMessage, appEx.Message);
        }

        [Given(@"a database error occurs when saving the organization ""(.*)""")]
        public async System.Threading.Tasks.Task GivenADatabaseErrorOccursWhenSavingTheOrganization(string orgName)
        {
            await GivenTheRoleExistsInTheDatabase("Admin");

            var dbMockForError = new Mock<OBDbContext>();
            dbMockForError.Setup(db => db.RoleTables).Returns(_db.RoleTables);
            dbMockForError.Setup(db => db.UserTables).Returns(_db.UserTables);

            dbMockForError.Setup(db => db.OrganizationTables!.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("Simulated DB error on organization save"));
            dbMockForError.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("Simulated DB error on save changes for org"));

            _loginService = new LoginService(
                dbMockForError.Object,
                _bCryptServiceMock.Object,
                _rsaServiceMock.Object
            );
        }

        [Given(@"a database error occurs when saving the user ""(.*)""")]
        public async System.Threading.Tasks.Task GivenADatabaseErrorOccursWhenSavingTheUser(string email)
        {
            await GivenTheRoleExistsInTheDatabase("Admin");
            // We are saving a temporary organization here using the _db instance.
            await GivenTheOrganizationIsSuccessfullySaved($"TempOrgForUserError_{Guid.NewGuid()}");

            var dbMockForError = new Mock<OBDbContext>();
            dbMockForError.Setup(db => db.RoleTables).Returns(_db.RoleTables); // This returns the *real* tables from _db
            dbMockForError.Setup(db => db.OrganizationTables).Returns(_db.OrganizationTables); // This returns the *real* tables from _db

            _loginService = new LoginService(
                dbMockForError.Object,
                _bCryptServiceMock.Object,
                _rsaServiceMock.Object
            );
        }

        [Given(@"the ""(.*)"" role exists in the database")]
        public async System.Threading.Tasks.Task GivenTheRoleExistsInTheDatabase(string roleName)
        {
            var existingRole = await _db.RoleTables.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (existingRole == null)
            {
                var role = TestDataFactory.CreateRole(roleName);
                _db.RoleTables!.Add(role);
                await _db.SaveChangesAsync();
                _db.Entry(role).State = EntityState.Detached;
            }
        }

        [Given(@"the ""(.*)"" role does not exist in the database")]
        public void GivenTheRoleDoesNotExitInTheDatabase(string roleName)
        {
            // No action needed, as the in-memory DB starts empty and we don't add this role.
        }

        [When(@"a new account is created with email ""(.*)"", password ""(.*)"", and organization ""(.*)""")]
        public async System.Threading.Tasks.Task WhenANewAccountIsCreatedWithEmailPasswordAndOrganization(string email, string password, string orgName)
        {
            _accountAndOrgDto = TestDataFactory.CreateAccountAndOrgDto(email, password, orgName);
            try
            {
                _caughtException = null;
                Validator.ValidateObject(_accountAndOrgDto, new ValidationContext(_accountAndOrgDto), validateAllProperties: true);
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
                _db.Entry(org).State = EntityState.Detached;
            }
        }

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

        // These 'Given' steps are purely for documentation in the feature file.
        // The actual validation is handled by DTO attributes.
        [Given(@"the application email minimum length is (\d+)")]
        [Given(@"the application email maximum length is (\d+)")]
        [Given(@"the application password minimum length is (\d+)")]
        [Given(@"the application password maximum length is (\d+)")]
        [Given(@"the application password requires complexity")]
        [Given(@"the application organization name minimum length is (\d+)")]
        [Given(@"the application organization name maximum length is (\d+)")]
        public void GivenTheApplicationConstraintIs(int length)
        {
            // No code needed here, as the validation is handled by DTO attributes
        }


        [Then(@"a ValidationException with message ""(.*)"" should be thrown")]
        public void ThenAValidationExceptionWithMessageShouldBeThrown(string expectedMessage)
        {
            Assert.NotNull(_caughtException);
            var validationEx = Assert.IsType<ValidationException>(_caughtException);
            Assert.Equal(expectedMessage, validationEx.Message);
        }

        [Then(@"a UnauthorizedAccessException with message ""(.*)"" should be thrown")]
        public void ThenAUnauthorizedAccessExceptionWithMessageShouldBeThrown(string expectedMessage)
        {
            Assert.NotNull(_caughtException);
            var validationEx = Assert.IsType<UnauthorizedAccessException>(_caughtException);
            Assert.Equal(expectedMessage, validationEx.Message);
        }

        // Helper method for generating long strings (useful for max length tests)
        private string GenerateString(int length, char character = 'a')
        {
            return new string(character, length);
        }
    }
}