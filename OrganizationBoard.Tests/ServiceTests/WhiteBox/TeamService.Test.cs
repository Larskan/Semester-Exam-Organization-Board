using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.Service;
using Moq;
using OrganizationBoard.DTO;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class TeamServiceTest
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

        // Removed Duplicate tests.

        #region Tests for CreateTeam
        // Test: Leader as valid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_Returns403_IfUserNotAdmin()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderTest");
            var service = new TeamService(context);

            // Act
            var result = await service.CreateTeam(new TeamDto { TeamName = "Dev Team" }, 3); //Not a leader ID

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        // Test: User as valid admin, creating new team
        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_ReturnsSuccess_IfUserIsLeaderAndExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidAdminTest");
            var service = new TeamService(context);

            // Act
            var result = await service.CreateTeam(new TeamDto { TeamID = 3, TeamName = "Success Team" }, 1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Team created successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("Success Team", result.Data.TeamName);
        }

        // Test: Exception in try/catch = 500
        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_Returns500_IfExceptionOccurs()
        {
            // Arrange
            var context = GetInMemoryDbContext("ExceptionTest");
            var service = new TeamService(context);

            // Act
            var result = await service.CreateTeam(new TeamDto { TeamName = null }, 1); // This will cause an exception because TeamName is null

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        #endregion Tests for CreateTeam

        #region Tests for UpdateTeam
        // Test: Leader invalid
        [Fact]
        public async System.Threading.Tasks.Task UpdateTeam_Returns403_IfUserNotLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderUpdateTest");
            var service = new TeamService(context);

            // Act
            var result = await service.UpdateTeamName(new TeamDto { TeamName = "Team 1" }, 3);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }
        // Test: existingTeam as null = 404
        [Fact]
        public async System.Threading.Tasks.Task UpdateTeam_Returns404_IfTeamNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotFoundTest");
            var service = new TeamService(context);

            // Act
            var result = await service.UpdateTeamName(new TeamDto { TeamID = 999, TeamName = "Nonexistent Team" }, 2);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // Test: existingTeam as valid team
        [Fact]
        public async System.Threading.Tasks.Task UpdateTeam_ReturnsSuccess_IfTeamExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidUpdateTest");
            var service = new TeamService(context);

            // Act
            var result = await service.UpdateTeamName(new TeamDto { TeamID = 2, TeamName = "Updated Team" }, 4);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Team updated successfully.", result.Message);
            Assert.Equal("Updated Team", result.Data.TeamName);
        }

        #endregion Tests for UpdateTeam

        #region Tests for DeleteTeam
        // Test: Team as valid team
        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_ReturnsSuccess_IfTeamDeleted()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidDeleteTest");
            var service = new TeamService(context);

            // Act
            var result = await service.DeleteTeam(2, 1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Team deleted successfully.", result.Message);
        }
        #endregion Tests for DeleteTeam

        #region Tests for GetTeamMembers
        // Test: Leader as valid user but trying to view another team, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_Returns403_IfUserNotMemberOfTeam()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotMemberGetMembersTest");
            var service = new TeamService(context);

            // Act
            var result = await service.GetTeamMembers(2, 2); // User 1 is leader, but isn't in team 2

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }
        // Test: Members.Count == 0 = 404
        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_Returns404_IfNoMembersFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("NoMembersTest");

            // Adding a team with no members
            context.TeamTables.Add(new Team { TeamID = 5, TeamName = "Empty Team" });
            await context.SaveChangesAsync();
            var service = new TeamService(context);

            // Act
            var result = await service.GetTeamMembers(5, 1); // Team 5 has no members

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Test: Successful test
        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_ReturnsSuccess_IfMembersFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidGetMembersTest");

            // Add users to team 1
            context.UserTables.Add(new User { UserID = 99, RoleID = 3, Email = "Test99@email.com", Password = "1234", OrganizationID = 1, TeamID = 1 });
            await context.SaveChangesAsync();
            var service = new TeamService(context);

            // Act
            var result = await service.GetTeamMembers(1, 2); // Team 1 has members

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Members retrieved successfully.", result.Message);
            Assert.NotEmpty(result.Data);
        }
    
        #endregion Tests for GetTeamMembers

        #region Tests for AssignUserToTeam
        // Test: userToAssign as null = 404
        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns404_IfUserNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("AssignUserToTeamUserNotFoundTest");
            var service = new TeamService(context);

            // Act
            var result = await service.AssignUserToTeam(1, 999, 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Test: userToAssign.TeamID != null = 400
        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns400_IfUserAlreadyInTeam()
        {
            // Arrange
            var context = GetInMemoryDbContext("AssignUserToTeamUserNotFoundTest");
            var service = new TeamService(context);

            // Act
            var result = await service.AssignUserToTeam(2, 3, 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }
        // Test: Successful Test
        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_ReturnsSuccess_IfUserAndTeamValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidAssignTest");
            var service = new TeamService(context);

            // Act
            var result = await service.AssignUserToTeam(1, 5, 1); // Assign user 5 to team 1

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("User assigned to team successfully.", result.Message);
        }

        #endregion Tests for AssignUserToTeam

        #region Tests for RemoveUserFromTeam
        // Test: Successful run
        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_ReturnsSuccess_IfUserAndTeamValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidRemoveTest");
            var service = new TeamService(context);

            // Act
            var result = await service.RemoveUserFromTeam(1, 3, 1); // Remove user 3 from team 1

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("User removed from team successfully.", result.Message);
        }
        #endregion Tests for RemoveUserFromTeam

    }

}