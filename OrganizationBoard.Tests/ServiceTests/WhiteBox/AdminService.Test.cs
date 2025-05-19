using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.Service;
using Moq;
using OrganizationBoard.DTO;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class AdminServiceTest
    {
        private OBDbContext GetInMemoryDbContext(string dbName = "TeamServiceTests")
        {
            var options = new DbContextOptionsBuilder<OBDbContext>()
                .UseInMemoryDatabase(databaseName: dbName).Options;

            var context = new OBDbContext(options);
            // The Seed Data from EFrameWork.Data/OBDbContext is not automatically reached, so we have to add some data manually.
            context.TeamTables.AddRange(
                new Team { TeamID = 1, TeamName = "Team 1" },
                new Team { TeamID = 2, TeamName = "Team 2" }
            );
            context.UserTables.AddRange(
                new User { UserID = 1, RoleID = 1, Email = "Test1@email.com", Password = "1234", OrganizationID = 1 }, //Admin
                new User { UserID = 2, RoleID = 2, Email = "Test2@email.com", Password = "1234", OrganizationID = 1, TeamID = 1 }, // Leader
                new User { UserID = 3, RoleID = 3, Email = "Test3@email.com", Password = "1234", OrganizationID = 1, TeamID = 1 }, // Member
                new User { UserID = 4, RoleID = 2, Email = "Test4@email.com", Password = "1234", OrganizationID = 1, TeamID = 2 },  // Leader
                new User { UserID = 5, RoleID = 3, Email = "Test5@email.com", Password = "1234", OrganizationID = 1 } //Member without team
            );
            context.SaveChanges();
            return context;
        }

        #region Tests for CreateUser
        // 2 Decisions = 3 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: Admin as valid user, creating new user
        // Test: Exception in try/catch = 500
        #endregion Tests for CreateUser

        #region Tests for UpdateUser
        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: existingUser as null = 404
        // Test: existingUser as valid user
        // Test: failing to update user = 500
        #endregion Tests for UpdateUser

        #region Tests for DeleteUser
        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: existingUser as null = 404
        // Test: existingUser as valid user, deleting user. 
        // Test: failing to update user = 500
        #endregion Tests for DeleteUser

        #region Tests for GetUser
        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: Admin as valid user, getting user
        // Test: user as null = 404
        // Test: failing to get user = 500
        #endregion Tests for GetUser

        #region Tests for GetAllUsers
        // 2 Decisions = 3 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: Admin as valid user, getting all users
        // Test: Exception in try/catch = 500
        #endregion Tests for GetAllUsers

        #region Tests for UpdateOrganization
        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: existingOrg as null = 404
        // Test: existingOrg as valid org
        // Test: failing to update org = 500
        #endregion Tests for UpdateOrganization
    }
}