using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.Service;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class TeamServiceTest
    {
        // From seeded users:
        /*  UserID = 1 has roleID 1(Admin)
            UserID = 2 has roleID 2(Leader) and TeamID = 2
            UserID = 3 has roleID 3(member) and TeamID = 1. */
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
                new User { UserID = 5, RoleID = 3, Email = "Test5@email.com", Password = "1234", OrganizationID = 1 }, //Member without team
                new User { UserID = 2, RoleID = 2, Email = "Test2@email.com", Password = "1234", OrganizationID = 1, TeamID = 1 }, // Leader
                new User { UserID = 3, RoleID = 3, Email = "Test3@email.com", Password = "1234", OrganizationID = 1, TeamID = 1 }, // Member
                new User { UserID = 4, RoleID = 2, Email = "Test4@email.com", Password = "1234", OrganizationID = 1, TeamID = 2 }  // Leader
            );
            context.SaveChanges();
            return context;
        }

        #region Tests for CreateTeam
        // Test: Leader as valid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_Returns403_IfUserNotLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderTest");
            var service = new TeamService(context);

            // Act
            var result = await service.CreateTeam(new Team { TeamName = "Dev Team" }, 3); //Not a leader ID

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        // Test: User as valid leader, creating new team //FIXME
        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_ReturnsSuccess_IfUserIsLeaderAndExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidLeaderTest");
            var service = new TeamService(context);

            // Act
            var result = await service.CreateTeam(new Team { TeamName = "Success Team" }, 4);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Team created successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("Success Team", result.Data.TeamName);
        }

        // Test: Exception in try/catch = 500//FIXME
        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_Returns500_IfExceptionOccurs()
        {
            // Arrange
            var context = GetInMemoryDbContext("ExceptionTest");

            // Adding a team manually with same ID that will be created.
            context.TeamTables.Add(new Team { TeamID = 2, TeamName = "Dupe Team" });
            await context.SaveChangesAsync();
            var service = new TeamService(context);

            // Act
            var result = await service.CreateTeam(new Team { TeamName = "Dupe Team" }, 4);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        #endregion Tests for CreateTeam

        #region Tests for UpdateTeam
        // Test: Leader as valid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task UpdateTeam_Returns403_IfUserNotLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderUpdateTest");
            var service = new TeamService(context);

            // Act
            var result = await service.UpdateTeam(new Team { TeamName = "Dev Team" }, 3);

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
            var result = await service.UpdateTeam(new Team { TeamID = 999, TeamName = "Nonexistent Team" }, 2);

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
            var result = await service.UpdateTeam(new Team { TeamID = 2, TeamName = "Updated Team" }, 4);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Team updated successfully.", result.Message);
            Assert.Equal("Updated Team", result.Data.TeamName);
        }

        // Test: failing to update team = 500 //FIXME
        [Fact]
        public async System.Threading.Tasks.Task UpdateTeam_Returns500_IfExceptionOccurs()
        {
            // Arrange
            var context = GetInMemoryDbContext("ExceptionUpdateTest");
            context.TeamTables.Add(new Team { TeamID = 2, TeamName = "Team 3" });
            await context.SaveChangesAsync();
            context.Dispose();
            var service = new TeamService(context);

            // Act
            var result = await service.UpdateTeam(new Team { TeamID = 2, TeamName = "Team 3" }, 4);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
        #endregion Tests for UpdateTeam

        #region Tests for DeleteTeam
        // Test: Leader as valid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_Returns403_IfUserNotLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderDeleteTest");
            var service = new TeamService(context);

            // Act
            var result = await service.DeleteTeam(1, 3);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }
        // Team as null = 404
        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_Returns404_IfTeamNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("DeleteTeamNotFoundTest");
            var service = new TeamService(context);

            // Act
            var result = await service.DeleteTeam(999, 2);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Team as valid team
        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_ReturnsSuccess_IfTeamDeleted()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidDeleteTest");
            var service = new TeamService(context);

            // Act
            var result = await service.DeleteTeam(2, 4);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Team deleted successfully.", result.Message);
        }
        // Exception in try/catch = 500//FIXME
        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_Returns500_IfExceptionOccurs()
        {
            // Arrange
            var context = GetInMemoryDbContext("DeleteTeamExceptionTest");

            context.TeamTables.Add(new Team { TeamID = 5, TeamName = "Locked Team" });
            await context.SaveChangesAsync();
            var service = new TeamService(context);

            // Simulating an exception by making it readonly or disposed.
            context.Dispose();

            // Act
            var result = await service.DeleteTeam(5, 4);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
        #endregion Tests for DeleteTeam

        #region Tests for GetTeamMembers
        // Test: Leader as valid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_Returns403_IfUserNotLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderGetMembersTest");
            var service = new TeamService(context);

            // Act
            var result = await service.GetTeamMembers(1, 3);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }
        // Test: Member as valid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_Returns403_IfUserNotMember()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotMemberGetMembersTest");
            var service = new TeamService(context);

            // Act
            var result = await service.GetTeamMembers(2, 1); // User 1 is leader, but isn't in team 2

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }
        // Test: Members as null and members.Count == 0 = 404//FIXME
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
            var result = await service.GetTeamMembers(5, 4); // Team 5 has no members

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Test: Members as valid members
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
        // Test: Exception in try/catch = 500//FIXME
        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_Returns500_IfExceptionOccurs()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetMembersExceptionTest");
            var service = new TeamService(context);
            context.Dispose();

            // Act
            var result = await service.GetTeamMembers(1, 2);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
        #endregion Tests for GetTeamMembers

        #region Tests for AssignUserToTeam
        // Test: Leader as valid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns403_IfUserNotLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderAssignTest");
            var service = new TeamService(context);

            // Act
            var result = await service.AssignUserToTeam(1, 2, 3);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }
        // Test: team as null = 404
        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns404_IfTeamNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("AssignUserToTeamNotFoundTest");
            var service = new TeamService(context);

            // Act
            var result = await service.AssignUserToTeam(999, 3, 2); // Nonexistent team

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Test: userToAssign as null = 404
        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns404_IfUserNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("AssignUserToTeamUserNotFoundTest");
            var service = new TeamService(context);

            // Act
            var result = await service.AssignUserToTeam(1, 999, 2);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Test: team and userToAssign as valid
        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_ReturnsSuccess_IfUserAndTeamValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidAssignTest");
            var service = new TeamService(context);

            // Act
            var result = await service.AssignUserToTeam(1, 5, 2); // Assign user 5 to team 1

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("User assigned to team successfully.", result.Message);
        }
        // Test: Exception in try/catch = 500//FIXME
        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns500_IfExceptionOccurs()
        {
            // Arrange
            var context = GetInMemoryDbContext("AssignUserToTeamExceptionTest");
            var service = new TeamService(context);
            context.Dispose();

            // Act
            var result = await service.AssignUserToTeam(1, 5, 2);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
        #endregion Tests for AssignUserToTeam

        #region Tests for RemoveUserFromTeam
        // Test: Leader as valid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_Returns403_IfUserNotLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderRemoveTest");
            var service = new TeamService(context);

            // Act
            var result = await service.RemoveUserFromTeam(1, 2, 3);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }
        // Test: team as null = 404
        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_Returns404_IfTeamNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("RemoveUserFromTeamNotFoundTest");
            var service = new TeamService(context);

            // Act
            var result = await service.RemoveUserFromTeam(999, 3, 2); // Nonexistent team

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Test: userToAssign as null = 404
        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_Returns404_IfUserNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("RemoveUserFromTeamUserNotFoundTest");
            var service = new TeamService(context);

            // Act
            var result = await service.RemoveUserFromTeam(1, 999, 2);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
        // Test: team and userToAssign as valid
        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_ReturnsSuccess_IfUserAndTeamValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidRemoveTest");
            var service = new TeamService(context);

            // Act
            var result = await service.RemoveUserFromTeam(1, 3, 2); // Remove user 3 from team 1

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("User assigned to team successfully.", result.Message);
        }
        // Test: Exception in try/catch = 500//FIXME
        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_Returns500_IfExceptionOccurs()
        {
            // Arrange
            var context = GetInMemoryDbContext("RemoveUserFromTeamExceptionTest");
            var service = new TeamService(context);
            context.Dispose();

            // Act
            var result = await service.RemoveUserFromTeam(1, 3, 2);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
        #endregion Tests for RemoveUserFromTeam

    }

}