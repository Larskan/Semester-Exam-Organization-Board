using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.Service;
using Moq;
using OrganizationBoard.DTO;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class AdminServiceTest // Done: 0/12
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
            context.BoardTables.AddRange(
                new Board { BoardID = 1, BoardName = "Board 1", TeamID = 1 },
                new Board { BoardID = 2, BoardName = "Board 2", TeamID = 2 }
            );
            context.TaskTables.AddRange(
                new EFrameWork.Model.Task { TaskID = 1, Title = "Task 1", BoardID = 1, StatusID = 1 },
                new EFrameWork.Model.Task { TaskID = 2, Title = "Task 2", BoardID = 1, StatusID = 2 },
                new EFrameWork.Model.Task { TaskID = 3, Title = "Task 3", BoardID = 2, StatusID = 3 }
            );
            context.SaveChanges();
            return context;
        }

        #region Duplicate Tests(Where many methods have the same test)
        // Why?
        // At some point it becomes copy paste code, in a true organization, having all tests would be fine, but for our scope, we can focus on the unique tests and show the dupes once.

        // The Admin as valid user, set to False = 403
        // Present at: All methods - Tests Saved: 5

        // The Exception in try/catch = 500
        // Present at: All methods - Tests saved: 5

        // User as null and return 404
        // Present at: UpdateUser, DeleteUser, GetUser - Tests Saved: 2

        // Email doesnt exist = 400
        // Present at: CreateUser, UpdateUser - Tests Saved: 1

        #endregion Duplicate Tests(Where many methods have the same test)

        #region Tests for CreateUser
        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: Email doesnt exist = 400
        // Test: Admin as valid user and email exists, creating new user
        // Test: Exception in try/catch = 500
        #endregion Tests for CreateUser

        #region Tests for UpdateUser
        // 5 Decisions = 6 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: existingUser as null = 404
        // Test: Email doesnt exist = 400
        // Test: New email matches existing email = 400
        // Test: Successsfully updating user
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