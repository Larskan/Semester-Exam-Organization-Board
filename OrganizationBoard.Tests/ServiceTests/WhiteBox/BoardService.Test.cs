using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.Service;
using Moq;
using OrganizationBoard.DTO;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class BoardServiceTest // Done: 0/58
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

        #region Tests for CreateBoard
        // 3 Decisions = 4 Tests
        // Test: Leader as valid user, set to False = 403.
        // Test: User as null, return 404
        // Test: User as valid user, creating new board
        // Test: Exception in try/catch = 500
        #endregion Tests for CreateBoard

        #region Tests for GetBoardTasks
        // 4 Decisions = 5 Tests
        // Test: Team Member access, set to False = 403.
        // Test: Tasks as null, return 404
        // Test: Tasks as empty, return 404
        // Test: Success, getting all tasks. 
        // Test: Exception in try/catch = 500
        #endregion Tests for GetBoardTasks

        #region Tests for UpdateBoard
        // 4 Decisions = 5 Tests.
        // Test: existingBoard as null, return 404
        // Test: Team Leader access, denied, return 403
        // Test: Team Member access, denied, return 403
        // Test: Successfully updated board
        // Test: Exception in try/catch = 500
        #endregion Tests for UpdateBoard

        #region Tests for DeleteBoard
        // 5 Decisions = 6 Tests.
        // Test: Board as null, return 404
        // Test: Team Leader access, denied, return 403
        // Test: Team Member access, denied, return 403
        // Test: Successfully deleted board
        // Test: Board has tasks, delete tasks first
        // Test: Exception in try/catch = 500
        #endregion Tests for DeleteBoard

        #region Tests for GetBoard
        // 4 Decisions = 5 Tests
        // Test: Board as null, return 404
        // Test: TeamID as null, return 404
        // Test: Team Member access, denied, return 403
        // Test: Successfully retrieved board
        // Test: Exception in try/catch = 500
        #endregion Tests for GetBoard

        #region Tests for GetTeamBoards
        // 4 Decisions = 5 Tests
        // Test: Team Member access, set to False = 403.
        // Test: Boards as null, return 404
        // Test: Boards as empty, return 404
        // Test: Success, getting all boards.
        // Test: Exception in try/catch = 500
        #endregion Tests for GetTeamBoards

        #region Tests for CreateTask
        // 3 Decisions = 4 Tests
        // Test: Board as null, return 404
        // Test: Team Leader access, denied, return 403
        // Test: Create new task successfully
        // Test: Exception in try/catch = 500
        #endregion Tests for CreateTask

        #region Tests for GetTask
        // 2 Decisions = 3 Tests
        // Test: Task as null, return 404
        // Test: Successfully retrieved task
        // Test: Exception in try/catch = 500
        #endregion Tests for GetTask

        #region Tests for UpdateTask
        // 3 Decisions = 4 Tests
        // Test: Leader as valid user, set to False = 403.
        // Test: existingTask as null = 404
        // Test: Successfully updated task
        // Test: Exception in try/catch = 500
        #endregion Tests for UpdateTask

        #region Tests for DeleteTask
        // 3 Decisions = 4 Tests
        // Test: Leader as valid user, set to False = 403.
        // Test: task as null = 404
        // Test: Successfully deleted task
        // Test: Exception in try/catch = 500
        #endregion Tests for DeleteTask

        #region Tests for AssignTask
        // 4 Decisions = 5 Tests
        // Test: Leader as valid user, set to False = return 403.
        // Test: task as null = 404
        // Test: user as null = 404
        // Test: Successfully assigned task
        // Test: Exception in try/catch = 500
        #endregion Tests for AssignTask

        #region Tests for MarkTaskAsComplete
        // 3 Decisions = 4 Tests
        // Test: Task as null, return 404
        // Test: IsUserInTask fails, return 403
        // Test: Successfully marked task as complete
        // Test: Exception in try/catch = 500
        #endregion Tests for MarkTaskAsComplete

        #region ConfirmTaskCompletion
        // 3 Decisions = 4 Tests
        // Test: Team Leader access, set to False = 403.
        // Test: task as null = 404
        // Test: Successfully confirmed task completion
        // Test: Exception in try/catch = 500
        #endregion ConfirmTaskCompletion


    }
}